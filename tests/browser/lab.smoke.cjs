// Real browser UI checks. Install Playwright separately; no production JS dependencies are required.
const path=require('node:path');
const fs=require('node:fs');
const assert=require('node:assert/strict');
const {chromium}=require(process.env.PLAYWRIGHT_MODULE||path.resolve(__dirname,'../../artifacts/browser-qa/node_modules/playwright'));
const output=path.resolve(__dirname,'../../artifacts/browser-qa');fs.mkdirSync(output,{recursive:true});
const base=process.argv[2]||process.env.STEM_BROWSER_URL||'http://localhost:5080';
const executable=process.env.STEM_BROWSER_EXE||(process.platform==='win32'?'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe':undefined);

(async()=>{
 const browser=await chromium.launch({executablePath:executable,headless:true});
 const context=await browser.newContext({viewport:{width:1512,height:982},deviceScaleFactor:1,acceptDownloads:true});
 const page=await context.newPage(),errors=[],failedRequests=[],passed=[];
 page.on('pageerror',e=>errors.push(e.message));
 page.on('console',msg=>{if(msg.type()==='error')errors.push(msg.text());});
 page.on('requestfailed',req=>failedRequests.push(req.url()+': '+req.failure()?.errorText));
 const ready=async()=>{await page.waitForSelector('body[data-ready="true"]',{timeout:30000});await page.waitForFunction(()=>!document.querySelector('.workspace').hasAttribute('aria-busy'));if(page.viewportSize().width>720){if(!await page.locator('.library').isVisible())await page.locator('#library-toggle').click();if(!await page.locator('.inspector').isVisible())await page.locator('#inspector-toggle').click();}};
 const diagnostic=()=>page.evaluate(()=>({workspace:window.labDiagnostics.workspace,readings:window.labDiagnostics.readings,circuit:window.labDiagnostics.circuit,rendered:window.labDiagnostics.renderedTools}));
 const check=(condition,message)=>{assert.ok(condition,message);passed.push(message);};
 const near=(actual,expected)=>Math.abs(actual-expected)<1e-9;
 const command=async click=>{
   const [response]=await Promise.all([page.waitForResponse(r=>r.url().endsWith('/api/command')&&r.request().method()==='POST'),click()]);
   assert.equal(response.status(),200,await response.text());await ready();
 };
 const shot=async name=>{await page.waitForTimeout(250);await page.screenshot({path:path.join(output,name+'.png'),fullPage:true});};
 const select=async id=>{await page.locator('#selected-tool').selectOption(id);await page.waitForFunction(id=>window.labDiagnostics.workspace[window.labDiagnostics.workspace.ActiveExperiment==='chemistry'?'Chemistry':'Electricity'].SelectedId===id,id);};
 const importWorkspace=async workspace=>{
   const response=page.waitForResponse(r=>r.url().endsWith('/api/command')&&r.request().method()==='POST');
   await page.locator('#import-file').setInputFiles({name:'test-draft.json',mimeType:'application/json',buffer:Buffer.from(JSON.stringify(workspace))});
   assert.equal((await response).status(),200);await page.waitForFunction(()=>document.querySelector('#status').textContent.includes('imported')||document.querySelector('#status').textContent.includes('استيراد'));await ready();
 };
 try{
   await page.goto(base,{waitUntil:'networkidle'});await ready();
   let d=await diagnostic();check(d.rendered===4,'Initial four chemistry tools render through WebGL');
   const graphics=await page.evaluate(()=>{const gl=document.querySelector('#viewport canvas').getContext('webgl2');const ext=gl.getExtension('WEBGL_debug_renderer_info');return ext?gl.getParameter(ext.UNMASKED_RENDERER_WEBGL):gl.getParameter(gl.RENDERER);});
   await shot('chemistry-browser');
   await command(()=>page.locator('#transfer').click());d=await diagnostic();check(near(d.readings.beaker.concentration,2)&&near(d.workspace.Chemistry.Tools.find(t=>t.Id==='beaker').VolumeL,.025),'UI transfers 25 mL with conserved 2 mol/L concentration');
   await select('water');await page.locator('#transfer-target').selectOption('beaker');await page.locator('#transfer-volume').fill('75');
   await command(()=>page.locator('#transfer').click());d=await diagnostic();check(near(d.readings.beaker.concentration,.5),'UI water dilution produces 0.5 mol/L');
   await select('beaker');check((await page.locator('.reading-number').first().textContent()).includes('0.5'),'Inspector displays calculated concentration');
   await command(()=>page.locator('#undo').click());d=await diagnostic();check(near(d.readings.beaker.concentration,2),'Undo restores the prior scientific state');
   await select('water');await page.locator('#transfer-target').selectOption('beaker');await page.locator('#transfer-volume').fill('75');await command(()=>page.locator('#transfer').click());
   await page.reload({waitUntil:'networkidle'});await ready();d=await diagnostic();check(near(d.readings.beaker.concentration,.5)&&d.workspace.ChemistryUndo.length===2,'Reload restores both workspace and undo history from browser storage');
   await select('stock');const beforeMove=d.workspace.Chemistry.Tools.find(t=>t.Id==='stock');
   const point=await page.evaluate(()=>window.labDiagnostics.project('stock'));
   await page.mouse.move(point.x,point.y);await page.mouse.down();await page.mouse.move(point.x-50,point.y-50,{steps:12});
   await command(()=>page.mouse.up());d=await diagnostic();const afterMove=d.workspace.Chemistry.Tools.find(t=>t.Id==='stock');check(afterMove.X!==beforeMove.X||afterMove.Z!==beforeMove.Z,'Dragging the real 3D bottle changes its validated bench position');
   await command(()=>page.locator('#rotate').click());d=await diagnostic();check(near(d.workspace.Chemistry.Tools.find(t=>t.Id==='stock').Yaw,Math.PI/6),'Rotate changes the actual selected 3D tool pose');
   const savedChemistry=structuredClone(d.workspace);
   await command(()=>page.locator('#electricity-tab').click());await page.locator('#presets').click();await command(()=>page.locator('[data-preset="led"]').click());
   d=await diagnostic();check(near(d.readings.extra.branch.CurrentA,7/1010),'LED preset gives the shared piecewise operating-point current');
   await shot('circuit-browser');
   await page.locator('#presets').click();await command(()=>page.locator('[data-preset="switch-lamp"]').click());
   await command(()=>page.locator('#toggle-switch').click());d=await diagnostic();check(d.circuit.status==='OpenCircuit'&&near(d.readings.resistor.branch.CurrentA,0),'Opening the switch stops calculated lamp current');
   await command(()=>page.locator('#undo').click());d=await diagnostic();check(near(d.readings.resistor.branch.CurrentA,.09),'Undo restores the closed lamp circuit');
   await command(async()=>{await page.locator('#reset').click();await page.locator('#confirm-dialog button[value="confirm"]').click();});await page.waitForTimeout(300);
   let a=await page.evaluate(()=>window.labDiagnostics.project('battery','a')),b=await page.evaluate(()=>window.labDiagnostics.project('resistor','a'));
   await page.mouse.click(a.x,a.y);await page.waitForSelector('#wire-pending:not([hidden])');await command(()=>page.mouse.click(b.x,b.y));
   d=await diagnostic();check(d.workspace.Electricity.Wires.some(w=>w.A==='battery.a'&&w.B==='resistor.a'),'Ray-picking actual 3D terminals creates a real electrical lead');
   for(const [first,second] of [['resistor.b','battery.b'],['multimeter.a','resistor.a'],['multimeter.b','resistor.b']]){
     await page.locator('#terminal-a').selectOption(first);await page.locator('#terminal-b').selectOption(second);await command(()=>page.locator('#add-wire').click());
   }
   await select('multimeter');d=await diagnostic();check(near(d.readings.multimeter.instrument.value,9),'Keyboard-accessible terminal selectors complete the circuit and meter reads 9 V');
   await command(()=>page.locator('#meter-mode').selectOption('current'));d=await diagnostic();check(d.circuit.status==='ShortCircuit'&&d.readings.multimeter.instrument.value===null,'A meter placed across the supply in current mode reports a short without invented readings');
   await command(()=>page.locator('#undo').click());
   await page.locator('#equipment-search').fill('XOR');await command(()=>page.locator('[data-add="xor"]').click());
   await command(()=>page.locator('[data-logic="0"]').click());d=await diagnostic();let gate=d.workspace.Electricity.Tools.find(t=>t.Kind==='xor');check(d.readings[gate.Id].output===true,'XOR trainer responds to A=1, B=0');
   await command(()=>page.locator('[data-logic="1"]').click());d=await diagnostic();check(d.readings[gate.Id].output===false,'XOR trainer and truth table respond to A=1, B=1');
   await command(()=>page.locator('#chemistry-tab').click());await page.locator('#elements').click();
   check(await page.locator('[data-element]').count()===118,'Periodic table contains all 118 selectable elements');
   await page.locator('#element-search').fill('Cu');await page.locator('[data-element="Cu"]').click();
   check((await page.locator('#element-detail').textContent()).includes('63.55'),'Periodic table shows the attributed copper reference mass');
   await shot('periodic-browser');await command(()=>page.locator('#place-element').click());d=await diagnostic();let sample=d.workspace.Chemistry.Tools.find(t=>t.Kind==='element');
   check(near(d.readings[sample.Id].mass,.6355),'Placed copper sample has calculated mass 0.6355 g');
   await page.locator('#equipment-search').fill('Erlenmeyer');await command(()=>page.locator('[data-add="erlenmeyer"]').click());d=await diagnostic();check(d.workspace.Chemistry.Tools.some(t=>t.Kind==='erlenmeyer'),'Searchable library adds distinct flask geometry and scientific state');
   const downloadPromise=page.waitForEvent('download');await page.locator('#export').click();const download=await downloadPromise;const notebook=JSON.parse(await fs.promises.readFile(await download.path(),'utf8'));const exported=notebook.workspace??notebook;
   check(exported.Chemistry.Tools.some(t=>t.Kind==='element')&&exported.Electricity.Tools.some(t=>t.Kind==='xor'),'Exported JSON contains both expanded benches');
   await command(async()=>{await page.locator('#reset').click();await page.locator('#confirm-dialog button[value="confirm"]').click();});await importWorkspace(exported);d=await diagnostic();check(d.workspace.Chemistry.Tools.some(t=>t.Kind==='erlenmeyer'),'Import restores the exported expanded workspace');
   const invalid=structuredClone(exported);invalid.ChemistryModel='unsupported-future-model';const invalidResponse=page.waitForResponse(r=>r.url().endsWith('/api/command'));
   await page.locator('#import-file').setInputFiles({name:'bad.json',mimeType:'application/json',buffer:Buffer.from(JSON.stringify(invalid))});check((await invalidResponse).status()===400,'Unsupported imported scientific model is rejected');
   await page.waitForFunction(()=>document.querySelector('#status').classList.contains('error'));d=await diagnostic();check(d.workspace.Chemistry.Tools.some(t=>t.Kind==='erlenmeyer'),'Rejected import preserves the current bench');
   // The intentional HTTP 400 is expected and may also be logged as a browser console resource error.
   const expectedErrorIndex=errors.findIndex(s=>s.includes('400'));if(expectedErrorIndex>=0)errors.splice(expectedErrorIndex,1);
   const expandedDraft=structuredClone(d.workspace);
   const catalog=await (await context.request.get(base+'/api/catalog')).json();
   let renderedKinds=0;
   for(const experiment of ['chemistry','electricity']){
     const definitions=catalog.tools.filter(tool=>experiment==='chemistry'?['vessels','reagents','instruments','apparatus','elements'].includes(tool.Category):tool.Kind==='meter'||['sources','passive','semiconductors','electrical-meters','logic','circuit-reference'].includes(tool.Category));
     for(let i=0;i<definitions.length;i+=8){
       let workspace=(await (await context.request.get(base+'/api/workspace')).json()).workspace;
       workspace.ActiveExperiment=experiment;const key=experiment==='chemistry'?'Chemistry':'Electricity';workspace[key].Tools=[];workspace[key].SelectedId='';
       for(const [index,def] of definitions.slice(i,i+8).entries()){
         const response=await context.request.post(base+'/api/command',{data:{Action:'add',Workspace:workspace,Kind:def.Kind}});assert.equal(response.status(),200,await response.text());workspace=(await response.json()).workspace;
         const tool=workspace[key].Tools.at(-1);tool.X=-2.7+(index%4)*1.8;tool.Z=index<4?-.7:.7;
       }
       await importWorkspace(workspace);d=await diagnostic();assert.equal(d.rendered,workspace[key].Tools.length);renderedKinds+=d.rendered;
       await shot(`${experiment}-catalog-${i}-browser`);
     }
   }
   check(renderedKinds===53,'All 52 unique equipment models render; the shared meter is exercised in both labs');
   await importWorkspace(expandedDraft);
   await page.locator('#language').click();check(await page.locator('html').getAttribute('dir')==='rtl','Arabic control switches the interface to RTL');
   await shot('arabic-browser');await page.locator('#elements').click();await shot('periodic-arabic-browser');await page.locator('#periodic-dialog .close-dialog').click();
   await page.setViewportSize({width:390,height:844});await page.waitForTimeout(300);
   check(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),'Mobile Arabic layout has no page-level horizontal overflow');
   if(!await page.locator('.library').isVisible())await page.locator('#library-toggle').click();check(await page.locator('.library').isVisible(),'Equipment library is accessible on a narrow viewport');await shot('mobile-arabic-browser');
   await page.locator('#elements').click();check(await page.locator('#periodic-dialog').isVisible(),'Periodic table remains accessible on mobile');await shot('mobile-periodic-browser');await page.locator('#periodic-dialog .close-dialog').click();
   await page.setViewportSize({width:1512,height:982});await page.locator('#language').click();
   await page.locator('#quality').click();await page.locator('#camera-reset').click();await page.locator('#help').click();check(await page.locator('#help-dialog').isVisible(),'Help and graphics controls work');await page.locator('#help-dialog .close-dialog').click();
   await importWorkspace(savedChemistry);await page.reload({waitUntil:'networkidle'});await ready();
   check((await diagnostic()).workspace.Chemistry.Tools.length===4,'Final reload restores a validated browser draft');
   check(errors.length===0,'No unexpected browser console or JavaScript errors');check(failedRequests.length===0,'All browser requests and local assets load successfully');
   const report={success:true,version:'0.5.0-browser',browser:await browser.version(),graphics,url:base,passed,errors,failedRequests,scope:'Isolated headless Edge UI and WebGL smoke; not school-hardware or long-session qualification.'};
   fs.writeFileSync(path.join(output,'browser-result.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report,null,2));
 }catch(error){fs.writeFileSync(path.join(output,'browser-failure.json'),JSON.stringify({success:false,error:error.stack,passed,errors,failedRequests},null,2));await page.screenshot({path:path.join(output,'failure.png'),fullPage:true}).catch(()=>{});throw error;}
 finally{await context.close();await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
