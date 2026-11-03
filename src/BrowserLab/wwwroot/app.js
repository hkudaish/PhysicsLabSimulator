import { LabScene } from './scene.js';

const $=id=>document.getElementById(id);
const escape=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const KEY='virtual-stem-browser-workspace-v1';
const words={
  'Learning through discovery':'التعلّم عبر الاكتشاف','Local workspace':'مساحة عمل محلية','EXPLORE · EXPERIMENT · UNDERSTAND':'استكشف · جرّب · افهم','Your science, hands-on.':'العلوم بين يديك.','Export draft':'تصدير التجربة','Import draft':'استيراد التجربة','Chemistry lab':'مختبر الكيمياء','Circuit lab':'مختبر الدارات','Equipment':'الأدوات','Periodic table':'الجدول الدوري','Circuit presets':'دارات جاهزة','Equipment library':'خزانة الأدوات','Choose a tool. Make it your experiment.':'اختر أداة وابدأ تجربتك.','tools on bench':'أداة على المنضدة','Preparing your laboratory…':'نجهّز مختبرك…','Drag tools to move · Drag empty bench to orbit · Scroll to zoom':'اسحب الأدوات لتحريكها · اسحب المنضدة للدوران · مرّر للتقريب','Instrument panel':'لوحة القياس','LIVE':'مباشر','Ideal models · Offline':'نماذج مثالية · دون إنترنت','A little curiosity. A world of discovery.':'قليل من الفضول، وعالم من الاكتشاف.','THE BUILDING BLOCKS OF MATTER':'اللبنات الأساسية للمادة','Place virtual sample':'إضافة عينة افتراضية','PubChem reference values. Virtual samples; chemical reactions are not simulated.':'قيم مرجعية من PubChem. عينات افتراضية؛ لا نحاكي التفاعلات الكيميائية.','START WITH A CONNECTION':'ابدأ بالتوصيل','Load a wired experiment. Undo brings your previous bench back.':'حمّل دارة موصلة. التراجع يستعيد المنضدة السابقة.','A guide to your laboratory':'دليل مختبرك','Cancel lead':'إلغاء السلك',
  'All equipment':'جميع الأدوات','Vessels / volumetry':'أوعية وقياس الحجم','Instruments':'أجهزة القياس','Apparatus reference':'أدوات مرجعية','Solutions':'المحاليل','Sources':'المصادر','Passive components':'العناصر السلبية','Diodes / LEDs':'الثنائيات و LED','Electrical instruments':'أجهزة كهربائية','Logic trainers':'مدرب المنطق','Circuit reference':'مرجع الدارات','3D reference':'مرجع ثلاثي الأبعاد','DC steady state':'حالة DC مستقرة','Truth-table trainer':'مدرب جدول الحقيقة','No matching equipment.':'لا توجد أدوات مطابقة.','Selected equipment':'الأداة المحددة','Choose equipment on the bench to inspect it.':'اختر أداة على المنضدة لفحصها.','Concentration':'التركيز','Volume':'الحجم','Capacity':'السعة','Solute':'المذاب','Pure water':'ماء نقي','Reference mass':'الكتلة المرجعية','Amount':'كمية المادة','Voltage':'الجهد','Current':'التيار','Power':'القدرة','Apply':'تطبيق','Transfer to':'النقل إلى','Transfer volume (mL)':'حجم النقل (mL)','Transfer solution':'نقل المحلول','Measurement target':'العينة المستهدفة','No target selected':'لم تختر عينة','Move closer to the target.':'حرّك الجهاز قرب العينة.','Unavailable for this solution.':'غير متاح لهذا المحلول.','Rotate':'تدوير','Remove':'إزالة','Bench position':'الموقع على المنضدة','Open switch':'فتح المفتاح','Close switch':'إغلاق المفتاح','Resistance (Ω)':'المقاومة (Ω)','Source voltage (V)':'جهد المصدر (V)','Forward drop (V)':'الجهد الأمامي (V)','Amount (mol)':'كمية المادة (mol)','Capacitance (F)':'السعة (F)','Inductance (H)':'المحاثة (H)','Meter mode':'نمط الملتيميتر','Add lead':'توصيل سلك','Connections':'التوصيلات','Click two colored terminals, or use the terminal selectors.':'انقر طرفين ملونين أو اختر الأطراف أدناه.','No leads yet.':'لا توجد توصيلات بعد.','Closed':'مغلق','Open':'مفتوح','Stored charge':'الشحنة المخزنة','Output':'الخرج','Model limits':'حدود النموذج','Ready to explore.':'المختبر جاهز للاستكشاف.','Saved in this browser':'محفوظ في هذا المتصفح','Draft exported.':'تم تصدير التجربة.','Draft imported.':'تم استيراد التجربة.','Experiment updated.':'تم تحديث التجربة.','Click a second terminal to complete the lead.':'انقر الطرف الثاني لإكمال التوصيل.','Select source':'اختر المصدر','Select destination':'اختر الوجهة','Open chemistry lab to place a sample':'افتح مختبر الكيمياء لإضافة العينة','Series resistors':'مقاومتان على التوالي','Parallel resistors':'مقاومتان على التوازي','Switch and lamp':'مصباح ومفتاح','LED with series resistor':'LED ومقاومة حماية','Capacitor — steady DC':'مكثف — DC مستقر','Two 1 kΩ resistors · 4.5 mA':'مقاومتان 1 kΩ · تيار 4.5 mA','Two 1 kΩ branches · 18 mA total':'فرعان 1 kΩ · تيار إجمالي 18 mA','Toggle the switch and inspect lamp power':'بدّل المفتاح وراقب قدرة المصباح','Piecewise junction · 1 kΩ protection':'نموذج مقطعي · مقاومة حماية 1 kΩ','Open branch in steady state · Q = CV':'فرع مفتوح في الحالة المستقرة · Q = CV','Graphics: realistic':'الرسوم: واقعية','Graphics: low':'الرسوم: اقتصادية',
  'Solved':'محلولة','OpenCircuit':'دارة مفتوحة','FloatingNetwork':'شبكة عائمة','ShortCircuit':'قصر كهربائي','InconsistentSources':'مصادر متعارضة','NumericallySingular':'دارة غير محددة','EmptyCircuit':'أضف مصدراً','InvalidInput':'مدخلات غير صالحة','Circuit status':'حالة الدارة','Rated power exceeded.':'تجاوز القدرة الاسمية.','Fixed 25 °C model temperature.':'درجة نموذج ثابتة 25 °C.','Virtual tared balance: mass = amount × reference atomic mass.':'ميزان افتراضي مصفّر: الكتلة = كمية المادة × الكتلة الذرية المرجعية.','Inspectable 3D reference. Its process is not simulated.':'مرجع ثلاثي الأبعاد قابل للفحص؛ لا نحاكي عمليته.','Ideal pH only for water, HCl and NaOH at 25 °C.':'pH مثالي للماء و HCl و NaOH فقط عند 25 °C.','No heating or chemical reactions. Different solutes cannot be mixed.':'لا نحاكي التسخين أو التفاعلات؛ لا يمكن خلط مذابات مختلفة.','Full 25 mL aliquots only. Fill the volumetric pipette first.':'تسليم 25 mL كاملة فقط. املأ الماصة الحجمية أولاً.','DC open branch. No charging transient; Q = CV when voltage is defined.':'فرع مفتوح في DC. لا نحاكي الشحن الزمني؛ Q = CV إذا عُرف الجهد.','DC ideal conductor. No inductive transient.':'موصل مثالي في DC؛ لا نحاكي السلوك الحثي الزمني.','Piecewise forward drop + on resistance; 1 GΩ off branch. No reverse breakdown.':'هبوط أمامي + مقاومة تشغيل؛ مقاومة إيقاف 1 GΩ. لا انهيار عكسي.','Fixed-resistance lamp. Brightness follows calculated power.':'مصباح بمقاومة ثابتة؛ السطوع يتبع القدرة المحسوبة.','Standalone Boolean trainer; no analog wiring terminals.':'مدرب منطقي مستقل؛ دون أطراف توصيل تناظرية.','Reference values':'قيم مرجعية','Atomic number':'العدد الذري','Atomic mass':'الكتلة الذرية','State':'الحالة','Electron configuration':'التوزيع الإلكتروني','Save unavailable; export your draft to keep it.':'تعذر الحفظ؛ صدّر التجربة للاحتفاظ بها.','Export before closing if browser storage is unavailable.':'صدّر التجربة قبل الإغلاق إذا لم يتوفر تخزين المتصفح.','Move, measure, discover':'حرّك وقِس واكتشف','Geometry and colors are illustrative; calculations use the shared C# engine.':'الأشكال والألوان توضيحية؛ الحسابات من محرك C# المشترك.'
};
let language='en',workspace,catalog,readings={},circuit,scene,busy=false,wireStart=null,selectedElement,lowQuality=false,restored=false,storageSaved=false;
Object.assign(words,{'Multimeter':'ملتيميتر','Temperature':'درجة الحرارة','Ideal solute':'مذاب مثالي'});
try{language=localStorage.getItem('virtual-stem-language')||'en';}catch{}
const t=text=>language==='ar'?(words[text]||text):text;
const chemical=()=>workspace.ActiveExperiment==='chemistry';
const state=()=>chemical()?workspace.Chemistry:workspace.Electricity;
const definition=kind=>catalog.tools.find(d=>d.Kind===kind);
const fluid=tool=>definition(tool.Kind)?.Behavior==='fluid';
const electrical=tool=>tool.Kind==='meter'||['sources','passive','semiconductors','electrical-meters'].includes(definition(tool.Kind)?.Category);
const logic=tool=>definition(tool.Kind)?.Behavior==='logic';
const num=(value,digits=5)=>value==null?'—':value===0?'0':Math.abs(value)<.0001||Math.abs(value)>=100000?value.toExponential(3):Number(value.toPrecision(digits)).toString();
const toolName=tool=>tool.Kind==='element'?`${tool.Mode} · ${language==='ar'?catalog.elements.find(e=>e.Symbol===tool.Mode)?.Arabic:catalog.elements.find(e=>e.Symbol===tool.Mode)?.Name}`:tool.Kind==='stock'&&tool.SoluteId&&tool.SoluteId!=='ideal-solute'?tool.SoluteId:tool.Kind==='meter'&&!chemical()?t('Multimeter'):definition(tool.Kind)?.[language==='ar'?'Arabic':'English']||tool.Kind;
const identified=tool=>`${toolName(tool)} · ${state().Tools.filter(s=>s.Kind===tool.Kind).indexOf(tool)+1}`;
const categories={all:'All equipment',vessels:'Vessels / volumetry',instruments:'Instruments',apparatus:'Apparatus reference',reagents:'Solutions',sources:'Sources',passive:'Passive components',semiconductors:'Diodes / LEDs','electrical-meters':'Electrical instruments',logic:'Logic trainers','circuit-reference':'Circuit reference'};

function status(message,error=false){$('status').textContent=message;$('status').classList.toggle('error',error);}
async function request(action,fields={},source=workspace){
  const response=await fetch('/api/command',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({Action:action,Workspace:source,...fields})});
  const result=await response.json();if(!response.ok)throw new Error(result.error||`Request failed (${response.status})`);return result;
}
function save(){
  try{localStorage.setItem(KEY,JSON.stringify(workspace));storageSaved=true;$('save-state').textContent=t('Saved in this browser');}
  catch{storageSaved=false;$('save-state').textContent=t('Save unavailable; export your draft to keep it.');}
}
async function run(action,fields={}){
  if(busy)return false;busy=true;document.querySelector('.workspace').setAttribute('aria-busy','true');
  try{
    const result=await request(action,fields);workspace=result.workspace;readings=result.readings;circuit=result.circuit;
    save();render();status(t('Experiment updated.'));return true;
  }catch(error){status(error.message,true);if(scene)scene.update(state(),readings,catalog.tools,catalog.elements,language);return false;}
  finally{busy=false;document.querySelector('.workspace').removeAttribute('aria-busy');}
}
function selectTool(id){if(busy)return;state().SelectedId=id;scene?.select(id);renderInspector();save();}
function setLanguage(){
  document.documentElement.lang=language;document.documentElement.dir=language==='ar'?'rtl':'ltr';$('language').textContent=language==='ar'?'English':'العربية';
  document.querySelectorAll('[data-i18n]').forEach(node=>node.textContent=t(node.dataset.i18n));
  $('equipment-search').placeholder=language==='ar'?'ابحث عن أداة…':'Find equipment…';$('element-search').placeholder=language==='ar'?'ابحث بالرمز أو الاسم أو العدد الذري':'Search symbol, name or atomic number';
}
function icon(kind){
  const shapes=definition(kind)?.Behavior==='fluid'?['burette','pipette','volpipette','testtube'].includes(kind)?'<path d="M19 3v30q3 4 6 0V3M19 12h6M19 18h4M19 24h4"/>':kind==='erlenmeyer'||kind==='florence'?'<path d="M18 3h10m-8 0v13L10 32q-1 4 4 4h18q4 0 2-4L26 16V3M16 25h13"/>':'<path d="M10 5h25l-3 5v22q0 4-4 4H15q-4 0-4-4V5M12 22h20M27 12h5m-5 5h5m-5 10h5"/>':kind==='battery'?'<rect x="5" y="9" width="34" height="25" rx="3"/><path d="M12 9V6h4v3m14 0V6h4v3M11 19h9m-4-4v8m12-4h6"/>':kind==='resistor'||kind==='rheostat'?'<path d="M2 22h9m22 0h9"/><rect x="11" y="15" width="22" height="14" rx="4"/><path d="M17 15v14m5-14v14m5-14v14"/>':kind==='switch'?'<path d="M3 30h7m24 0h7M10 30L31 9"/><circle cx="10" cy="30" r="3"/><circle cx="34" cy="30" r="3"/>':kind==='bulb'||kind==='led'?'<path d="M17 29v6h10v-6M17 32h10M16 25a12 12 0 1 1 12 0l-1 4H17zM19 22l3-7 3 7"/>':kind==='capacitor'?'<path d="M5 22h13m8 0h13M18 10v24m8-24v24"/>':kind==='inductor'?'<path d="M2 26h5c-7-22 12-22 5 0c-7-22 12-22 5 0c-7-22 12-22 5 0c-7-22 12-22 5 0h15"/>':kind==='diode'?'<path d="M3 22h10v-9l15 9-15 9v-9m15-10v20m0-10h13"/>':definition(kind)?.Behavior==='logic'?'<path d="M10 8h12q18 14 0 28H10zM3 15h7M3 28h7m20-7h10"/>':kind==='meter'||kind==='phmeter'||kind==='ammeter'||kind==='voltmeter'||kind==='thermometer'?'<rect x="10" y="3" width="25" height="35" rx="3"/><path d="M14 9h17v8H14z"/><circle cx="22" cy="27" r="5"/>':kind==='balance'?'<path d="M9 36h26M22 36V8M7 12h30M8 12L3 27h10zm28 0-5 15h10z"/>':kind==='element'?'<circle cx="22" cy="21" r="3"/><ellipse cx="22" cy="21" rx="19" ry="7"/><ellipse cx="22" cy="21" rx="19" ry="7" transform="rotate(60 22 21)"/><ellipse cx="22" cy="21" rx="19" ry="7" transform="rotate(120 22 21)"/>':'<path d="M7 36h30M29 36V4M29 13H10m0 0v16M6 22h8"/><circle cx="10" cy="30" r="5"/>';
  return `<svg viewBox="0 0 44 42" aria-hidden="true">${shapes}</svg>`;
}
function renderLibrary(reset=false){
  const select=$('category'),previous=reset?'all':select.value||'all';
  const groups=chemical()?['all','vessels','instruments','apparatus','reagents']:['all','sources','passive','semiconductors','electrical-meters','logic','circuit-reference'];
  select.innerHTML=groups.map(group=>`<option value="${group}">${escape(t(categories[group]))}</option>`).join('');select.value=groups.includes(previous)?previous:'all';
  const query=$('equipment-search').value.trim().toLowerCase(),category=select.value;
  let tools=catalog.tools.filter(def=>chemical()?['vessels','reagents','instruments','apparatus'].includes(def.Category):def.Kind==='meter'||['sources','passive','semiconductors','electrical-meters','logic','circuit-reference'].includes(def.Category));
  const total=tools.length+(chemical()?catalog.reagents.length:0);$('equipment-count').textContent=total;
  const entries=tools.map(def=>({kind:def.Kind,title:def.Kind==='meter'&&!chemical()?(language==='ar'?'ملتيميتر':'Multimeter'):def[language==='ar'?'Arabic':'English'],category:def.Kind==='meter'&&!chemical()?'electrical-meters':def.Category,note:def.Behavior==='reference'?'3D reference':def.Behavior==='steady'?'DC steady state':def.Behavior==='logic'?'Truth-table trainer':'',search:def.Arabic+' '+def.English}));
  if(chemical())for(const formula of catalog.reagents)entries.push({kind:'stock',content:formula,title:formula,category:'reagents',note:'0.1 mol/L · 100 mL',search:formula});
  const shown=entries.filter(entry=>(category==='all'||category===entry.category)&&(!query||entry.search.toLowerCase().includes(query)));
  $('equipment-list').innerHTML=shown.length?shown.map(entry=>`<button class="equipment-card" data-add="${entry.kind}" ${entry.content?`data-content="${escape(entry.content)}"`:''} ${state().Tools.length>=12?'disabled':''} aria-label="${escape((language==='ar'?'إضافة ':'Add ')+entry.title)}"><span class="add">+</span>${icon(entry.kind)}<span class="tool-title">${escape(entry.title)}</span>${entry.note?`<span class="note">${escape(t(entry.note))}</span>`:''}</button>`).join(''):`<p class="muted small">${escape(t('No matching equipment.'))}</p>`;
  $('bench-count').textContent=`${state().Tools.length} / 12`;
}
function description(tool){
  const def=definition(tool.Kind);
  if(def.Behavior==='reference')return 'Inspectable 3D reference. Its process is not simulated.';
  if(def.Behavior==='logic')return 'Standalone Boolean trainer; no analog wiring terminals.';
  return ({phmeter:'Ideal pH only for water, HCl and NaOH at 25 °C.',thermometer:'Fixed 25 °C model temperature.',balance:'Virtual tared balance: mass = amount × reference atomic mass.',volpipette:'Full 25 mL aliquots only. Fill the volumetric pipette first.',capacitor:'DC open branch. No charging transient; Q = CV when voltage is defined.',inductor:'DC ideal conductor. No inductive transient.',diode:'Piecewise forward drop + on resistance; 1 GΩ off branch. No reverse breakdown.',led:'Piecewise forward drop + on resistance; 1 GΩ off branch. No reverse breakdown.',bulb:'Fixed-resistance lamp. Brightness follows calculated power.'})[tool.Kind]||(fluid(tool)?'No heating or chemical reactions. Different solutes cannot be mixed.':'');
}
function field(label,content){const id=content.match(/\bid="([^"]+)"/)?.[1];return `<label class="field-label" ${id?`for="${id}"`:''}>${escape(t(label))}</label>${content}`;}
function row(label,value){return `<div class="property-row"><span>${escape(t(label))}</span><b>${escape(value)}</b></div>`;}
function readout(label,value,unit='',sub=''){return `<div class="reading"><div class="reading-label">${escape(t(label))}</div><div class="reading-number">${escape(num(value))} <small>${escape(unit)}</small></div>${sub?`<div class="reading-sub">${escape(t(sub))}</div>`:''}</div>`;}
function renderInspector(){
  const current=state(),tool=current.Tools.find(tool=>tool.Id===current.SelectedId);
  let html=field('Selected equipment',`<select id="selected-tool"><option value="">${escape(t('Choose equipment on the bench to inspect it.'))}</option>${current.Tools.map(s=>`<option value="${s.Id}" ${s.Id===tool?.Id?'selected':''}>${escape(identified(s))}</option>`).join('')}</select>`);
  if(!tool){$('inspector').innerHTML=html;return;}
  const def=definition(tool.Kind),reading=readings[tool.Id]||{},note=description(tool);
  html+=`<div class="selected-tool"><p class="eyebrow">${escape(t(categories[def.Category]||'Selected equipment'))}</p><h3>${escape(toolName(tool))}</h3>${note?`<p>${escape(t(note))}</p>`:''}</div>`;
  if(fluid(tool)){
    html+=readout('Concentration',reading.concentration,'mol/L',tool.SoluteId==='ideal-solute'?'Ideal solute':tool.SoluteId||'Pure water');
    html+=row('Volume',num(tool.VolumeL*1000)+' mL')+row('Capacity',num(tool.CapacityL*1000)+' mL');
    if(reading.ph!=null)html+=row('pH',num(reading.ph));
    const targets=current.Tools.filter(s=>s.Id!==tool.Id&&fluid(s));
    html+=field('Transfer to',`<select id="transfer-target">${targets.map(s=>`<option value="${s.Id}">${escape(identified(s))}</option>`).join('')}</select>`);
    html+=field('Transfer volume (mL)',`<input id="transfer-volume" type="number" min="0.1" max="500" step="0.1" value="25" style="width:100%"><button class="primary block" id="transfer" ${!targets.length?'disabled':''}>${escape(t('Transfer solution'))}</button>`);
  }
  if(tool.Kind==='element')html+=readout('Reference mass',reading.mass,'g')+row('Amount',num(tool.Value)+' mol');
  if(reading.instrument){
    const measure=reading.instrument;
    html+=readout(tool.Kind==='meter'?(chemical()?'Concentration':tool.Mode==='current'?'Current':'Voltage'):tool.Kind==='balance'?'Reference mass':tool.Kind==='phmeter'?'pH':tool.Kind==='thermometer'?'Temperature':tool.Kind==='ammeter'?'Current':'Voltage',measure.value,measure.unit);
    if(measure.value==null&&chemical())html+=`<p class="warning">${escape(t(!measure.nearby?'Move closer to the target.':'Unavailable for this solution.'))}</p>`;
  }
  if(chemical()&&['meter','phmeter','thermometer','balance'].includes(tool.Kind)){
    const targets=current.Tools.filter(s=>tool.Kind==='balance'?s.Kind==='element':fluid(s));
    html+=field('Measurement target',`<select id="measurement-target"><option value="">${escape(t('No target selected'))}</option>${targets.map(s=>`<option value="${s.Id}" ${s.Id===tool.Target?'selected':''}>${escape(identified(s))}</option>`).join('')}</select>`);
  }
  if(!chemical()){
    if(reading.branch){html+=readout('Current',reading.branch.CurrentA*1000,'mA')+row('Voltage',num(reading.branch.VoltageV)+' V')+row('Power',num(reading.branch.PowerW)+' W');if(reading.branch.Overloaded)html+=`<p class="warning">${escape(t('Rated power exceeded.'))}</p>`;}
    if(tool.Kind==='capacitor')html+=readout('Stored charge',reading.charge,'C')+row('Current','0 A (DC)');
    if(tool.Kind==='switch')html+=`<button class="primary block" id="toggle-switch">${escape(t(tool.Mode==='closed'?'Open switch':'Close switch'))}</button>`;
    if(logic(tool)){
      html+=readout('Output',reading.output?1:0);
      html+=`<div class="logic-inputs">${[0,...(tool.Kind==='not'?[]:[1])].map(bit=>`<button data-logic="${bit}" class="${tool.Mode[bit]==='1'?'on':''}">${bit===0?'A':'B'} = ${tool.Mode[bit]}</button>`).join('')}</div>`;
      const output=(a,b)=>({and:a&&b,or:a||b,not:!a,nand:!(a&&b),nor:!(a||b),xor:a!==b})[tool.Kind];
      html+=`<table class="truth-table"><thead><tr><th>A</th>${tool.Kind==='not'?'':'<th>B</th>'}<th>Y</th></tr></thead><tbody>${(tool.Kind==='not'?['00','10']:['00','01','10','11']).map(bits=>`<tr class="${bits[0]===tool.Mode[0]&&(tool.Kind==='not'||bits===tool.Mode)?'active':''}"><td>${bits[0]}</td>${tool.Kind==='not'?'':`<td>${bits[1]}</td>`}<td>${output(bits[0]==='1',bits[1]==='1')?1:0}</td></tr>`).join('')}</tbody></table>`;
    }
    if(tool.Kind==='meter')html+=field('Meter mode',`<select id="meter-mode"><option value="voltage" ${tool.Mode==='voltage'?'selected':''}>${escape(t('Voltage'))} (V)</option><option value="current" ${tool.Mode==='current'?'selected':''}>${escape(t('Current'))} (A)</option></select>`);
  }
  const limits=({battery:[0,12,.1,'Source voltage (V)'],resistor:[10,10000,10,'Resistance (Ω)'],rheostat:[10,10000,10,'Resistance (Ω)'],bulb:[10,10000,10,'Resistance (Ω)'],led:[.1,5,.1,'Forward drop (V)'],diode:[.1,5,.1,'Forward drop (V)'],capacitor:[.000001,1,.00001,'Capacitance (F)'],inductor:[.000001,10,.01,'Inductance (H)'],element:[.0001,1,.001,'Amount (mol)']})[tool.Kind];
  if(limits){const [min,max,step,label]=limits;html+=field(label,`<div class="input-row"><input id="component-value" aria-label="${escape(t(label))}" type="number" min="${min}" max="${max}" step="${step}" value="${tool.Value}"><button id="apply-value">${escape(t('Apply'))}</button></div>`);}
  html+=`<div class="inspector-actions"><button id="rotate">↻ ${escape(t('Rotate'))}</button><button id="remove" class="danger">× ${escape(t('Remove'))}</button></div>`;
  html+=`<details style="margin-top:12px"><summary class="muted small">${escape(t('Bench position'))}</summary><div class="input-row" style="margin-top:7px"><label class="small">X<input id="position-x" aria-label="X position" type="number" min="-3" max="3" step=".05" value="${tool.X}"></label><label class="small">Z<input id="position-z" aria-label="Z position" type="number" min="-1.5" max="1.5" step=".05" value="${tool.Z}"></label><button id="apply-position">${escape(t('Apply'))}</button></div></details>`;
  if(!chemical()){
    html+=`<div class="divider"></div>${row('Circuit status',t(circuit?.status||'EmptyCircuit'))}`;
    const terminals=current.Tools.filter(electrical).flatMap(s=>['a','b'].map(end=>({id:s.Id+'.'+end,label:identified(s)+' · '+(end==='a'?'A (+)':'B (−)')})));
    const options=terminals.map(s=>`<option value="${s.id}">${escape(s.label)}</option>`).join('');
    html+=`<p class="terminal-hint">${escape(t('Click two colored terminals, or use the terminal selectors.'))}</p><div class="connection-row"><select id="terminal-a" aria-label="First terminal">${options}</select><select id="terminal-b" aria-label="Second terminal">${options}</select></div><button id="add-wire" class="block">+ ${escape(t('Add lead'))}</button><label class="field-label">${escape(t('Connections'))} (${current.Wires.length})</label><div class="wire-list">${current.Wires.length?current.Wires.map(w=>`<div class="wire-row"><span>${escape(shortTerminal(w.A)+' ↔ '+shortTerminal(w.B))}</span><button data-unwire="${w.Id}" aria-label="Disconnect lead">×</button></div>`).join(''):`<span class="muted small">${escape(t('No leads yet.'))}</span>`}</div>`;
  }
  $('inspector').innerHTML=html;
  if($('terminal-b'))$('terminal-b').selectedIndex=Math.min(1,$('terminal-b').options.length-1);
}
function shortTerminal(id){const [toolId,end]=id.split('.');const tool=state().Tools.find(s=>s.Id===toolId);return tool?identified(tool)+'.'+end:id;}
function render(){
  for(const button of document.querySelectorAll('[data-lab]'))button.setAttribute('aria-selected',String(button.dataset.lab===workspace.ActiveExperiment));
  $('presets').hidden=chemical();$('scene-title').textContent=chemical()?(language==='ar'?'تخفيف المحاليل':'SOLUTION DILUTION'):(language==='ar'?'مختبر التيار المستمر':'DC CIRCUIT EXPLORER');
  $('undo').disabled=(chemical()?workspace.ChemistryUndo:workspace.ElectricityUndo).length===0;
  renderLibrary();renderInspector();scene?.update(state(),readings,catalog.tools,catalog.elements,language);renderWirePending();
  if(selectedElement)renderElementDetail();
}
function renderWirePending(){
  $('wire-pending').hidden=!wireStart;$('cancel-wire').hidden=!wireStart;
  if(wireStart)$('wire-pending').textContent=shortTerminal(wireStart)+' · '+t('Click a second terminal to complete the lead.');
}
async function terminal(id){if(busy)return;if(!wireStart){wireStart=id;renderWirePending();return;}const a=wireStart;wireStart=null;renderWirePending();if(a!==id)await run('wire',{A:a,B:id});}
function elementPosition(z){if(z<=2)return[1,z===1?1:18];if(z<=10)return[2,z<=4?z-2:z+8];if(z<=18)return[3,z<=12?z-10:z];if(z<=36)return[4,z-18];if(z<=54)return[5,z-36];if(z<=57)return[6,z-54];if(z<=71)return[9,z-54];if(z<=86)return[6,z-68];if(z<=89)return[7,z-86];if(z<=103)return[10,z-86];return[7,z-100];}
function renderElements(){
  const shades={'Alkali metal':'#edd6bf','Alkaline earth metal':'#ece1bd','Transition metal':'#e0e4b8','Post-transition metal':'#cddfc6','Metalloid':'#b9d7cb','Nonmetal':'#bfd8db','Halogen':'#d1cbe0','Noble gas':'#e4cbd7','Lanthanide':'#d3dfbd','Actinide':'#d9d5bf'};
  $('periodic-grid').innerHTML=catalog.elements.map(e=>{const [row,col]=elementPosition(e.Number);return `<button class="element-cell ${e.Symbol===selectedElement?.Symbol?'active':''}" data-element="${e.Symbol}" style="grid-row:${row};grid-column:${col};--element-color:${shades[e.Category]||'#dce3d0'}" title="${escape(language==='ar'?e.Arabic:e.Name)}" aria-label="${escape(e.Name+' '+e.Number)}"><small>${e.Number}</small>${e.Symbol}</button>`;}).join('');
  filterElements();renderElementDetail();
}
function filterElements(){const query=$('element-search').value.trim().toLowerCase();document.querySelectorAll('[data-element]').forEach(button=>{const e=catalog.elements.find(e=>e.Symbol===button.dataset.element);button.classList.toggle('dim',!!query&&!`${e.Name} ${e.Arabic} ${e.Symbol}`.toLowerCase().includes(query)&&String(e.Number)!==query);});}
function renderElementDetail(){
  const e=selectedElement;$('element-detail').innerHTML=`<div class="element-symbol">${e.Symbol}</div><div class="element-info"><h3>${escape(language==='ar'?e.Arabic:e.Name)} <small>· ${e.Number}</small></h3><p>${escape(t('Atomic mass'))}: <bdi>${escape(e.AtomicMass)}</bdi> · ${escape(e.State)} · ${escape(e.Category)}</p><p><bdi>${escape(e.Configuration)}</bdi></p></div>`;
  $('place-element').disabled=!chemical()||state().Tools.length>=12;$('place-element').textContent=t(chemical()?'Place virtual sample':'Open chemistry lab to place a sample');
}
function openPresets(){
  const presets=[['series','⇢','Series resistors','Two 1 kΩ resistors · 4.5 mA'],['parallel','⑂','Parallel resistors','Two 1 kΩ branches · 18 mA total'],['switch-lamp','☼','Switch and lamp','Toggle the switch and inspect lamp power'],['led','◉','LED with series resistor','Piecewise junction · 1 kΩ protection'],['capacitor','⊣','Capacitor — steady DC','Open branch in steady state · Q = CV']];
  $('preset-list').innerHTML=presets.map(([id,symbol,name,note])=>`<button class="preset-option" data-preset="${id}"><span>${symbol}</span><span><b>${escape(t(name))}</b><small>${escape(t(note))}</small></span><span style="margin-inline-start:auto">↗</span></button>`).join('');$('preset-dialog').showModal();
}
function showHelp(){
  $('help-content').innerHTML=language==='ar'?'<h3>حرّك وقِس واكتشف</h3><p>انقر أداة لاختيارها أو اخترها من لوحة القياس. اسحبها لتحريكها، واسحب مساحة فارغة أو استخدم زر الفأرة الأيمن للدوران، ومرّر للتقريب. يمكن تعديل الموقع رقمياً من لوحة القياس. التراجع يستعيد آخر 20 إجراءً.</p><h3>الكيمياء</h3><p>اختر وعاء المصدر ثم الهدف وحجم النقل. أضف الماء لتخفيف المحلول. املأ الماصة الحجمية قبل تسليم 25 mL كاملة. حدّد عينة لأجهزة القياس وحرّك الجهاز ضمن 1.65 وحدة منها. قِس كتلة عينات العناصر بميزان افتراضي مصفّر.</p><h3>الدارات</h3><p>انقر طرفين لإنشاء سلك، أو استخدم قوائم الأطراف. صِل الفولتميتر على التوازي والأميتر على التوالي. النمط الخاطئ قد يقصر المصدر. استخدم الدارات الجاهزة للبدء. بوابات المنطق مدربات مستقلة دون أطراف تناظرية.</p><h3>حدود النموذج</h3><p>التخفيف مثالي لمذاب واحد؛ لا تفاعلات أو تسخين. pH للماء و HCl و NaOH فقط عند 25 °C. المكثف والملف في حالة DC مستقرة دون محاكاة زمنية. LED والثنائي بنموذج مقطعي ومقاومة إيقاف 1 GΩ. الأدوات المرجعية ثلاثية الأبعاد غير وظيفية علمياً. ألوان السوائل توضيحية.</p><h3>الحفظ والخصوصية</h3><p>تحفظ التجربتان والتراجع في تخزين هذا المتصفح تلقائياً. استخدم تصدير التجربة لنسخة JSON قابلة للاستيراد. لا نكتب في قاعدة بيانات التطبيق المكتبي. الحفظ مرتبط بعنوان المتصفح والمنفذ؛ التصفح الخاص قد يحذفه. يعمل الخادم محلياً فقط.</p>':'<h3>Move, measure, discover</h3><p>Click a tool to select it, or choose it from the instrument panel. Drag tools to move them. Drag empty space or use the right mouse button to orbit, and scroll to zoom. Numeric coordinates are available in the inspector. Undo restores your last 20 actions.</p><h3>Chemistry</h3><p>Select a source vessel, then a destination and transfer volume. Dilute with water. Fill the volumetric pipette before dispensing its complete 25 mL aliquot. Choose a target for instruments and move within 1.65 scene units. The virtual tared balance calculates the mass of element samples.</p><h3>Circuits</h3><p>Click two terminals to create a lead, or use the terminal selectors. Connect a voltmeter in parallel and an ammeter in series. An incorrect current-meter connection can short the supply. Try the wired presets. Logic gates are standalone trainers without analog terminals.</p><h3>Model limits</h3><p>Ideal single-solute dilution; no reactions or heating. pH is available only for water, HCl and NaOH at 25 °C. Capacitors and inductors use DC steady state without transients. Diodes and LEDs use a piecewise model and a 1 GΩ off branch. Reference apparatus is labeled and does not simulate a process. Liquid colors are illustrative.</p><h3>Saving and privacy</h3><p>Both benches and undo history save automatically in this browser. Export a JSON draft as a portable backup and import it here later. The native desktop database is separate. Browser storage belongs to this address and port; private browsing may discard it. The server only listens locally.</p>';
  $('help-dialog').showModal();
}

function bind(){
  $('language').onclick=()=>{language=language==='ar'?'en':'ar';try{localStorage.setItem('virtual-stem-language',language);}catch{}setLanguage();render();renderElements();if($('save-state').textContent)$('save-state').textContent=t(storageSaved?'Saved in this browser':'Save unavailable; export your draft to keep it.');status(t('Ready to explore.'));};
  $('equipment-search').oninput=()=>renderLibrary();$('category').onchange=()=>renderLibrary();
  $('equipment-list').onclick=async event=>{const button=event.target.closest('[data-add]');if(button)await run('add',{Kind:button.dataset.add,Content:button.dataset.content||null});};
  for(const button of document.querySelectorAll('[data-lab]'))button.onclick=async()=>{if(busy||workspace.ActiveExperiment===button.dataset.lab)return;workspace.ActiveExperiment=button.dataset.lab;wireStart=null;$('category').value='all';$('equipment-search').value='';await run('view');};
  $('library-toggle').onclick=()=>{const wide=innerWidth>920;document.body.classList.toggle(wide?'library-collapsed':'library-open');$('library-toggle').setAttribute('aria-expanded',String(wide?!document.body.classList.contains('library-collapsed'):document.body.classList.contains('library-open')));};
  $('undo').onclick=()=>{wireStart=null;run('undo');};$('reset').onclick=()=>{wireStart=null;run('reset');};
  $('camera-reset').onclick=()=>scene?.resetCamera();$('quality').onclick=()=>{lowQuality=!lowQuality;scene?.quality(lowQuality);status(t(lowQuality?'Graphics: low':'Graphics: realistic'));};
  $('cancel-wire').onclick=()=>{wireStart=null;renderWirePending();};
  $('elements').onclick=()=>{renderElements();$('periodic-dialog').showModal();};$('presets').onclick=openPresets;$('help').onclick=showHelp;
  document.querySelectorAll('.close-dialog').forEach(button=>button.onclick=()=>button.closest('dialog').close());
  for(const dialog of document.querySelectorAll('dialog'))dialog.addEventListener('click',event=>{if(event.target===dialog){const r=dialog.getBoundingClientRect();if(event.clientX<r.left||event.clientX>r.right||event.clientY<r.top||event.clientY>r.bottom)dialog.close();}});
  $('element-search').oninput=filterElements;$('periodic-grid').onclick=event=>{const button=event.target.closest('[data-element]');if(button){selectedElement=catalog.elements.find(e=>e.Symbol===button.dataset.element);renderElements();}};
  $('place-element').onclick=async()=>{if(await run('add',{Kind:'element',Content:selectedElement.Symbol}))$('periodic-dialog').close();};
  $('preset-list').onclick=async event=>{const button=event.target.closest('[data-preset]');if(button&&await run('preset',{Content:button.dataset.preset})){$('preset-dialog').close();wireStart=null;renderWirePending();}};
  $('inspector').onclick=async event=>{
    const tool=state().Tools.find(s=>s.Id===state().SelectedId);if(!tool)return;
    const button=event.target.closest('button');if(!button)return;
    const fields={Tool:tool.Id};
    if(button.dataset.logic!=null){const bits=tool.Mode.split(''),bit=Number(button.dataset.logic);bits[bit]=bits[bit]==='1'?'0':'1';await run('mode',{...fields,Content:bits.join('')});}
    else if(button.dataset.unwire)await run('unwire',{Content:button.dataset.unwire});
    else if(button.id==='apply-value'){const input=$('component-value');if(input.reportValidity())await run('value',{...fields,Value:Number(input.value)});}
    else if(button.id==='transfer'){const input=$('transfer-volume');if(input.reportValidity())await run('transfer',{...fields,Target:$('transfer-target').value,Value:Number(input.value)});}
    else if(button.id==='toggle-switch')await run('mode',{...fields,Content:tool.Mode==='closed'?'open':'closed'});
    else if(button.id==='rotate')await run('rotate',fields);
    else if(button.id==='remove'){wireStart=null;await run('remove',fields);}
    else if(button.id==='apply-position'){if($('position-x').reportValidity()&&$('position-z').reportValidity())await run('move',{...fields,X:Number($('position-x').value),Z:Number($('position-z').value)});}
    else if(button.id==='add-wire'){wireStart=null;await run('wire',{A:$('terminal-a').value,B:$('terminal-b').value});}
  };
  $('inspector').onchange=event=>{
    const input=event.target,tool=state().Tools.find(s=>s.Id===state().SelectedId);
    if(input.id==='selected-tool'){selectTool(input.value);return;}
    if(input.id==='measurement-target'&&input.value&&tool)run('target',{Tool:tool.Id,Target:input.value});
    if(input.id==='meter-mode'&&tool)run('mode',{Tool:tool.Id,Content:input.value});
  };
  $('export').onclick=()=>{const url=URL.createObjectURL(new Blob([JSON.stringify(workspace,null,2)],{type:'application/json'}));const link=document.createElement('a');link.href=url;link.download='virtual-stem-browser-draft.json';link.click();setTimeout(()=>URL.revokeObjectURL(url),2000);status(t('Draft exported.'));};
  $('import').onclick=()=>$('import-file').click();$('import-file').onchange=async event=>{
    const file=event.target.files[0];event.target.value='';if(!file||busy)return;
    if(file.size>4*1024*1024){status('Draft is too large (maximum 4 MiB).',true);return;}
    busy=true;
    try{const candidate=JSON.parse(await file.text()),result=await request('view',{},candidate);workspace=result.workspace;readings=result.readings;circuit=result.circuit;wireStart=null;save();render();status(t('Draft imported.'));}
    catch(error){status('Draft could not be imported: '+error.message,true);}finally{busy=false;}
  };
  document.addEventListener('keydown',event=>{if(event.key==='Escape'){wireStart=null;renderWirePending();}if((event.ctrlKey||event.metaKey)&&event.key==='z'&&!['INPUT','SELECT','TEXTAREA'].includes(event.target.tagName)&&!document.querySelector('dialog[open]')){event.preventDefault();run('undo');}});
}
async function start(){
  setLanguage();
  const responses=await Promise.all([fetch('/api/catalog'),fetch('/api/workspace')]);
  if(responses.some(r=>!r.ok))throw new Error('The local C# server is not ready. Start it with python3 tools/browser_lab.py.');
  const [data,initial]=await Promise.all(responses.map(r=>r.json()));catalog=data;workspace=initial.workspace;readings=initial.readings;circuit=initial.circuit;
  let restoreError;
  try{const draft=localStorage.getItem(KEY);if(draft){const result=await request('view',{},JSON.parse(draft));workspace=result.workspace;readings=result.readings;circuit=result.circuit;restored=true;}}
  catch(error){restoreError=error.message;}
  await document.fonts.ready;
  try{scene=new LabScene($('viewport'),{select:selectTool,terminal,move:(id,x,z)=>run('move',{Tool:id,X:x,Z:z}),notice:message=>status(message,true)});}
  catch(error){$('viewport').innerHTML='<div class="loading"><p>WebGL 2 is unavailable. Enable hardware acceleration and reload.</p><p class="small">The equipment controls and calculated readings remain available.</p></div>';console.error(error);}
  selectedElement=catalog.elements.find(e=>e.Symbol==='Cu');bind();render();if(scene)$('loading')?.remove();
  status(restoreError?'Saved draft could not be restored. It remains in browser storage until your next successful edit. '+restoreError:restored?(language==='ar'?'تم استئناف التجربة المحفوظة.':'Saved workspace resumed.'):t('Ready to explore.'));
  // Read-only diagnostics support the browser smoke harness without bypassing UI commands.
  window.labDiagnostics={get workspace(){return structuredClone(workspace);},get readings(){return structuredClone(readings);},get circuit(){return structuredClone(circuit);},get renderedTools(){return scene?.groups.size||0;},project:(id,end)=>scene?.projected(id,end),get renderer(){return scene?.renderer.info.render;}};
  document.body.dataset.ready='true';
}
start().catch(error=>{status(error.message,true);$('loading').innerHTML=`<p>Could not start the laboratory.</p><p class="small">${escape(error.message)}</p>`;console.error(error);});
