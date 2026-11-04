import { LabScene } from './scene.js';
import { Guide,cleanLearning } from './guide.mjs';
import {context as lessonContext,text as lessonText} from './experiments.mjs';

const $=id=>document.getElementById(id);
const escape=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const KEY='virtual-stem-browser-workspace-v1';
const NOTEBOOK='virtual-stem-browser-notebook-v1';
const words={
  'Learning through discovery':'التعلّم عبر الاكتشاف','Local workspace':'مساحة عمل محلية','EXPLORE · EXPERIMENT · UNDERSTAND':'استكشف · جرّب · افهم','Your science, hands-on.':'العلوم بين يديك.','Export draft':'تصدير التجربة','Import draft':'استيراد التجربة','Chemistry lab':'مختبر الكيمياء','Circuit lab':'مختبر الدارات','Equipment':'الأدوات','Periodic table':'الجدول الدوري','Circuit presets':'دارات جاهزة','Equipment library':'خزانة الأدوات','Choose a tool. Make it your experiment.':'اختر أداة وابدأ تجربتك.','tools on bench':'أداة على المنضدة','Preparing your laboratory…':'نجهّز مختبرك…','Drag tools to move · Drag empty bench to orbit · Scroll to zoom':'اسحب الأدوات لتحريكها · اسحب المنضدة للدوران · مرّر للتقريب','Instrument panel':'لوحة القياس','LIVE':'مباشر','Ideal models · Offline':'نماذج مثالية · دون إنترنت','A little curiosity. A world of discovery.':'قليل من الفضول، وعالم من الاكتشاف.','THE BUILDING BLOCKS OF MATTER':'اللبنات الأساسية للمادة','Place virtual sample':'إضافة عينة افتراضية','PubChem reference values. Virtual samples; chemical reactions are not simulated.':'قيم مرجعية من PubChem. عينات افتراضية؛ لا نحاكي التفاعلات الكيميائية.','START WITH A CONNECTION':'ابدأ بالتوصيل','Load a wired experiment. Undo brings your previous bench back.':'حمّل دارة موصلة. التراجع يستعيد المنضدة السابقة.','A guide to your laboratory':'دليل مختبرك','Cancel lead':'إلغاء السلك',
  'All equipment':'جميع الأدوات','Vessels / volumetry':'أوعية وقياس الحجم','Instruments':'أجهزة القياس','Apparatus reference':'أدوات مرجعية','Solutions':'المحاليل','Sources':'المصادر','Passive components':'العناصر السلبية','Diodes / LEDs':'الثنائيات و LED','Electrical instruments':'أجهزة كهربائية','Logic trainers':'مدرب المنطق','Circuit reference':'مرجع الدارات','3D reference':'مرجع ثلاثي الأبعاد','DC steady state':'حالة DC مستقرة','Truth-table trainer':'مدرب جدول الحقيقة','No matching equipment.':'لا توجد أدوات مطابقة.','Selected equipment':'الأداة المحددة','Choose equipment on the bench to inspect it.':'اختر أداة على المنضدة لفحصها.','Concentration':'التركيز','Volume':'الحجم','Capacity':'السعة','Solute':'المذاب','Pure water':'ماء نقي','Reference mass':'الكتلة المرجعية','Amount':'كمية المادة','Voltage':'الجهد','Current':'التيار','Power':'القدرة','Apply':'تطبيق','Transfer to':'النقل إلى','Transfer volume (mL)':'حجم النقل (mL)','Transfer solution':'نقل المحلول','Measurement target':'العينة المستهدفة','No target selected':'لم تختر عينة','Move closer to the target.':'حرّك الجهاز قرب العينة.','Unavailable for this solution.':'غير متاح لهذا المحلول.','Rotate':'تدوير','Remove':'إزالة','Bench position':'الموقع على المنضدة','Open switch':'فتح المفتاح','Close switch':'إغلاق المفتاح','Resistance (Ω)':'المقاومة (Ω)','Source voltage (V)':'جهد المصدر (V)','Forward drop (V)':'الجهد الأمامي (V)','Amount (mol)':'كمية المادة (mol)','Capacitance (F)':'السعة (F)','Inductance (H)':'المحاثة (H)','Meter mode':'نمط الملتيميتر','Add lead':'توصيل سلك','Connections':'التوصيلات','Click two colored terminals, or use the terminal selectors.':'انقر طرفين ملونين أو اختر الأطراف أدناه.','No leads yet.':'لا توجد توصيلات بعد.','Closed':'مغلق','Open':'مفتوح','Stored charge':'الشحنة المخزنة','Output':'الخرج','Model limits':'حدود النموذج','Ready to explore.':'المختبر جاهز للاستكشاف.','Saved in this browser':'محفوظ في هذا المتصفح','Draft exported.':'تم تصدير التجربة.','Draft imported.':'تم استيراد التجربة.','Experiment updated.':'تم تحديث التجربة.','Click a second terminal to complete the lead.':'انقر الطرف الثاني لإكمال التوصيل.','Select source':'اختر المصدر','Select destination':'اختر الوجهة','Open chemistry lab to place a sample':'افتح مختبر الكيمياء لإضافة العينة','Series resistors':'مقاومتان على التوالي','Parallel resistors':'مقاومتان على التوازي','Switch and lamp':'مصباح ومفتاح','LED with series resistor':'LED ومقاومة حماية','Capacitor — steady DC':'مكثف — DC مستقر','Two 1 kΩ resistors · 4.5 mA':'مقاومتان 1 kΩ · تيار 4.5 mA','Two 1 kΩ branches · 18 mA total':'فرعان 1 kΩ · تيار إجمالي 18 mA','Toggle the switch and inspect lamp power':'بدّل المفتاح وراقب قدرة المصباح','Piecewise junction · 1 kΩ protection':'نموذج مقطعي · مقاومة حماية 1 kΩ','Open branch in steady state · Q = CV':'فرع مفتوح في الحالة المستقرة · Q = CV','Graphics: realistic':'الرسوم: واقعية','Graphics: low':'الرسوم: اقتصادية',
  'Solved':'محلولة','OpenCircuit':'دارة مفتوحة','FloatingNetwork':'شبكة عائمة','ShortCircuit':'قصر كهربائي','InconsistentSources':'مصادر متعارضة','NumericallySingular':'دارة غير محددة','EmptyCircuit':'أضف مصدراً','InvalidInput':'مدخلات غير صالحة','Circuit status':'حالة الدارة','Rated power exceeded.':'تجاوز القدرة الاسمية.','Fixed 25 °C model temperature.':'درجة نموذج ثابتة 25 °C.','Virtual tared balance: mass = amount × reference atomic mass.':'ميزان افتراضي مصفّر: الكتلة = كمية المادة × الكتلة الذرية المرجعية.','Inspectable 3D reference. Its process is not simulated.':'مرجع ثلاثي الأبعاد قابل للفحص؛ لا نحاكي عمليته.','Ideal pH only for water, HCl and NaOH at 25 °C.':'pH مثالي للماء و HCl و NaOH فقط عند 25 °C.','No heating or chemical reactions. Different solutes cannot be mixed.':'لا نحاكي التسخين أو التفاعلات؛ لا يمكن خلط مذابات مختلفة.','Full 25 mL aliquots only. Fill the volumetric pipette first.':'تسليم 25 mL كاملة فقط. املأ الماصة الحجمية أولاً.','DC open branch. No charging transient; Q = CV when voltage is defined.':'فرع مفتوح في DC. لا نحاكي الشحن الزمني؛ Q = CV إذا عُرف الجهد.','DC ideal conductor. No inductive transient.':'موصل مثالي في DC؛ لا نحاكي السلوك الحثي الزمني.','Piecewise forward drop + on resistance; 1 GΩ off branch. No reverse breakdown.':'هبوط أمامي + مقاومة تشغيل؛ مقاومة إيقاف 1 GΩ. لا انهيار عكسي.','Fixed-resistance lamp. Brightness follows calculated power.':'مصباح بمقاومة ثابتة؛ السطوع يتبع القدرة المحسوبة.','Standalone Boolean trainer; no analog wiring terminals.':'مدرب منطقي مستقل؛ دون أطراف توصيل تناظرية.','Reference values':'قيم مرجعية','Atomic number':'العدد الذري','Atomic mass':'الكتلة الذرية','State':'الحالة','Electron configuration':'التوزيع الإلكتروني','Save unavailable; export your draft to keep it.':'تعذر الحفظ؛ صدّر التجربة للاحتفاظ بها.','Export before closing if browser storage is unavailable.':'صدّر التجربة قبل الإغلاق إذا لم يتوفر تخزين المتصفح.','Move, measure, discover':'حرّك وقِس واكتشف','Geometry and colors are illustrative; calculations use the shared C# engine.':'الأشكال والألوان توضيحية؛ الحسابات من محرك C# المشترك.'
};
let language='en',workspace,catalog,readings={},circuit,scene,guide,busy=false,wireStart=null,selectedElement,lowQuality=false,restored=false,storageSaved=false,paused=false,rewiring=null;
let redo={chemistry:[],electricity:[]};
Object.assign(words,{'Multimeter':'ملتيميتر','Temperature':'درجة الحرارة','Ideal solute':'مذاب مثالي'});
Object.assign(words,{'Save experiment':'حفظ التجربة','Export notebook':'تصدير دفتر التجارب','Details':'التفاصيل','Free Mode':'الوضع الحر','Guided Mode':'الوضع الموجّه','Resume guide':'استئناف الدليل','Click to add · Drag to place':'انقر للإضافة · اسحب للوضع','Select':'اختيار','Move':'تحريك','Measure':'قياس','Pause':'إيقاف مؤقت','Resume':'استئناف','Guide':'الدليل','Paused — experiment editing is locked':'متوقف مؤقتاً — تعديل التجربة مقفل','LEARN THROUGH EXPERIMENTS':'تعلّم عبر التجارب','Both laboratories':'المختبران','All levels':'جميع المستويات','Beginner':'مبتدئ','Intermediate':'متوسط','Advanced':'متقدم','All types':'جميع الأنواع','Hands-on':'عملي','Concept lessons':'دروس مفاهيمية','Any duration':'أي مدة','All topics':'جميع الموضوعات','Measurement':'القياس','Concentration':'التركيز','Acids and bases':'الأحماض والقواعد','Thermal processes':'العمليات الحرارية','Reactions':'التفاعلات','DC fundamentals':'أساسيات DC','Semiconductors':'أشباه الموصلات','Capacitance':'السعة','VALIDATED':'تم التحقق','Cancel':'إلغاء','Continue':'متابعة','Duplicate':'نسخ الأداة','Duplicate empty vessel':'نسخ وعاء فارغ','Empty to waste':'إفراغ إلى النفايات','Fill from source':'ملء من مصدر','Available source':'المصدر المتاح','Fill volume (mL)':'حجم الملء (mL)','Fill':'ملء','Connect':'توصيل','Disconnect':'فصل','Inspect':'فحص','No valid steps can be skipped.':'لا يمكن تخطي الخطوات الإلزامية.'});
Object.assign(words,{'Empty vessel':'وعاء فارغ','Fixed model':'نموذج ثابت'});
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
  try{localStorage.setItem(NOTEBOOK,JSON.stringify(notebook()));storageSaved=true;$('save-state').textContent=t('Saved in this browser');}
  catch{storageSaved=false;$('save-state').textContent=t('Save unavailable; export your draft to keep it.');}
}
function notebook(){return {format:'virtual-stem-notebook-v1',workspace,learning:guide?.data??cleanLearning(),redo};}
function cleanRedo(value){const result={chemistry:[],electricity:[]};for(const lab of ['chemistry','electricity'])if(Array.isArray(value?.[lab]))result[lab]=value[lab].slice(-20).filter(s=>s&&s.Experiment===lab&&Array.isArray(s.Tools)&&s.Tools.length<=12&&Array.isArray(s.Wires)&&s.Wires.length<=64);return result;}
async function run(action,fields={}){
  if(paused&&action!=='view'){status(language==='ar'?'استأنف التعديل أولاً؛ نموذج المحاكاة في حالة مستقرة.':'Resume editing first; this scientific model is steady state.',true);scene?.update(state(),readings,catalog.tools,catalog.elements,language);return false;}
  if(busy)return false;busy=true;document.querySelector('.workspace').setAttribute('aria-busy','true');
  try{
    const before=structuredClone(state()),lab=workspace.ActiveExperiment;
    if(action==='redo'){if(!redo[lab].length)return false;fields={Snapshot:redo[lab].at(-1)};}
    const result=await request(action,fields);workspace=result.workspace;readings=result.readings;circuit=result.circuit;
    if(action==='undo'){redo[lab].push(before);if(redo[lab].length>20)redo[lab].shift();}else if(action==='redo')redo[lab].pop();else if(action!=='view')redo[lab]=[];
    if(action==='remove'||action==='reset'||action==='clear'||action==='preset'){wireStart=null;rewiring=null;}
    guide?.afterChange(action);save();render();status(t('Experiment updated.'));return true;
  }catch(error){status(error.message,true);if(scene)scene.update(state(),readings,catalog.tools,catalog.elements,language);return false;}
  finally{busy=false;document.querySelector('.workspace').removeAttribute('aria-busy');}
}
function selectTool(id){if(busy)return;state().SelectedId=id;scene?.select(id);renderInspector();renderContext();save();}
function panel(which,open){if(open&&innerWidth<=720){const other=which==='library'?'inspector':'library';document.body.classList.add(other==='library'?'library-collapsed':'inspector-collapsed');$(other==='library'?'library-toggle':'inspector-toggle').setAttribute('aria-expanded','false');}document.body.classList.toggle(which==='library'?'library-collapsed':'inspector-collapsed',!open);$(which==='library'?'library-toggle':'inspector-toggle').setAttribute('aria-expanded',String(open));}
function setPaused(value){paused=value;wireStart=null;rewiring=null;renderWirePending();$('pause-banner').hidden=!value;$('pause').classList.toggle('active',value);$('pause').setAttribute('aria-pressed',String(value));scene?.setEditable(!value);renderContext();}
async function switchLab(lab){if(busy)return false;if(workspace.ActiveExperiment===lab)return true;const previous=workspace.ActiveExperiment;workspace.ActiveExperiment=lab;wireStart=null;rewiring=null;$('category').value='all';$('equipment-search').value='';const ok=await run('view');if(!ok)workspace.ActiveExperiment=previous;return ok;}
function confirmAction(title,message){return new Promise(resolve=>{$('confirm-title').textContent=title;$('confirm-message').textContent=message;const dialog=$('confirm-dialog');dialog.returnValue='cancel';dialog.addEventListener('close',()=>resolve(dialog.returnValue==='confirm'),{once:true});dialog.showModal();});}
function setLanguage(){
  document.querySelectorAll('option[data-i18n]').forEach(node=>{if(!node.hasAttribute('value'))node.value=node.dataset.i18n;});
  document.documentElement.lang=language;document.documentElement.dir=language==='ar'?'rtl':'ltr';$('language').textContent=language==='ar'?'English':'العربية';
  document.querySelectorAll('[data-i18n]').forEach(node=>node.textContent=t(node.dataset.i18n));
  $('equipment-search').placeholder=language==='ar'?'ابحث عن أداة…':'Find equipment…';$('element-search').placeholder=language==='ar'?'ابحث بالرمز أو الاسم أو العدد الذري':'Search symbol, name or atomic number';
  $('guide-search').placeholder=language==='ar'?'ابحث بالتجربة أو الموضوع أو الأداة':'Search topic, equipment or experiment';
}
function guidedPlacement(kind,content){const s=guide?.step();return s?.focus==='library'&&s.kind===kind&&(s.content||null)===(content||null)&&s.placement?{X:s.placement[0],Z:s.placement[1]}:{};}
function highlightStep(step){
  document.querySelectorAll('.equipment-card.is-required').forEach(b=>b.classList.remove('is-required'));
  $('elements').classList.toggle('is-required',step?.kind==='element');
  if(step?.kind&&step.kind!=='element'){
    const buttons=[...document.querySelectorAll('[data-add]')].filter(b=>b.dataset.add===step.kind&&(b.dataset.content||null)===(step.content||null));
    if(step.focus==='library'&&!buttons.length){$('equipment-search').value='';$('category').value='all';renderLibrary();}
    [...document.querySelectorAll('[data-add]')].filter(b=>b.dataset.add===step.kind&&(b.dataset.content||null)===(step.content||null)).forEach(b=>b.classList.add('is-required'));
  }
  scene?.setGuideStep(step);
}
function focusStep(step){
  if(step.focus==='library'){
    if(step.kind==='element'){selectedElement=catalog.elements.find(e=>e.Symbol===step.content)||selectedElement;renderElements();$('periodic-dialog').showModal();highlightStep(step);return;}
    panel('library',true);$('equipment-search').value='';$('category').value='all';renderLibrary();highlightStep(step);document.querySelector('.equipment-card.is-required')?.scrollIntoView({block:'nearest'});return;
  }
  const targets=state().Tools.filter(s=>s.Kind===step.kind&&(!step.content||(s.Kind==='element'?s.Mode:s.SoluteId)===step.content));
  if(targets[step.index||0])selectTool(targets[step.index||0].Id);
  panel('inspector',true);highlightStep(step);
  const focus=step.focus==='transfer'?'transfer-target':step.focus==='value'?'component-value':step.focus==='connect'?'terminal-a':step.focus==='record'?'guide-record':step.focus==='toggle'?'toggle-switch':'measurement-target';
  $(focus)?.focus();
}
function renderContext(){
  const bar=$('context-toolbar'),tool=state().Tools.find(s=>s.Id===state().SelectedId);if(!tool){bar.hidden=true;return;}
  let actions=[];
  if(!paused){
    if(fluid(tool)){
      if(tool.VolumeL>0&&(tool.Kind!=='volpipette'||Math.abs(tool.VolumeL-.025)<1e-9))actions.push(['pour',language==='ar'?'صب':'Pour',true]);
      if(tool.VolumeL<tool.CapacityL)actions.push(['fill',t('Fill')]);
      actions.push(['measure',t('Measure')],['rotate',t('Rotate')]);
      if(tool.VolumeL>0)actions.push(['empty',t('Empty to waste')]);
    }else if(!chemical()&&electrical(tool)){
      actions.push(['connect',t('Connect'),true]);
      if(state().Wires.some(w=>w.A.startsWith(tool.Id+'.')||w.B.startsWith(tool.Id+'.')))actions.push(['disconnect',t('Disconnect')]);
      if(['battery','resistor','rheostat','bulb','diode','led','capacitor','inductor'].includes(tool.Kind))actions.push(['value',language==='ar'?'القيمة':'Value']);
      actions.push(['measure',t('Measure')],['rotate',t('Rotate')]);
    }else if(logic(tool))actions.push(['inputs',language==='ar'?'المدخلات':'Inputs',true],['rotate',t('Rotate')]);
    else if(tool.Kind==='element')actions.push(['value',t('Amount'),true],['measure',t('Measure')],['rotate',t('Rotate')]);
    else actions.push(['move',t('Move')],['rotate',t('Rotate')]);
  }
  actions.push(['details',t('Inspect')]);
  bar.innerHTML=`<div class="context-name"><span>${escape(toolName(tool))}</span><button data-context="deselect" aria-label="Deselect">×</button></div><div class="context-actions">${actions.map(([action,label,primary])=>`<button data-context="${action}" class="${primary?'context-main':''}">${escape(label)}</button>`).join('')}</div>`;
  bar.hidden=false;positionContext();
}
function positionContext(){
  const bar=$('context-toolbar');if(!scene||bar.hidden)return;const point=scene.contextPoint(state().SelectedId);if(!point){bar.hidden=true;return;}
  const rect=document.querySelector('.stage').getBoundingClientRect(),width=bar.offsetWidth,height=bar.offsetHeight;
  const x=Math.max(10,Math.min(rect.width-width-10,point.x-rect.left-width/2)),y=Math.max(55,Math.min(rect.height-height-100,point.y-rect.top-height-12));
  bar.style.transform=`translate(${x}px,${y}px)`;
}
async function contextual(action){
  const tool=state().Tools.find(s=>s.Id===state().SelectedId);if(!tool)return;
  if(action==='deselect'){selectTool('');return;}
  if(action==='rotate'){await run('rotate',{Tool:tool.Id});return;}
  if(action==='empty'){await run('empty',{Tool:tool.Id});return;}
  if(action==='disconnect'){await run('disconnect',{Tool:tool.Id});return;}
  if(action==='connect'){if(paused)return;rewiring=null;wireStart=tool.Id+'.a';renderWirePending();return;}
  if(action==='move'){$('move-mode').click();status(language==='ar'?'اسحب الأداة أو عدّل إحداثياتها من التفاصيل.':'Drag the selected tool, or set its coordinates in Details.');}
  panel('inspector',true);
  const target=action==='pour'?'transfer-target':action==='fill'?'fill-source':action==='value'?'component-value':action==='inputs'?null:chemical()?'measurement-target':'meter-mode';
  $(target)?.scrollIntoView({block:'nearest'});$(target)?.focus();
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
  $('equipment-list').innerHTML=shown.length?shown.map(entry=>`<button class="equipment-card" draggable="true" data-add="${entry.kind}" ${entry.content?`data-content="${escape(entry.content)}"`:''} ${state().Tools.length>=12||paused?'disabled':''} aria-label="${escape((language==='ar'?'إضافة ':'Add ')+entry.title)}"><span class="add">+</span>${icon(entry.kind)}<span class="tool-title">${escape(entry.title)}</span>${entry.note?`<span class="note">${escape(t(entry.note))}</span>`:''}</button>`).join(''):`<p class="muted small">${escape(t('No matching equipment.'))}</p>`;
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
    html+=readout('Concentration',reading.concentration,'mol/L',tool.VolumeL===0?'Empty vessel':tool.SoluteId==='ideal-solute'?'Ideal solute':tool.SoluteId||'Pure water');
    html+=row('Volume',num(tool.VolumeL*1000)+' mL')+row('Capacity',num(tool.CapacityL*1000)+' mL')+row('Solute',tool.VolumeL===0?'—':tool.SoluteId==='ideal-solute'?t('Ideal solute'):tool.SoluteId||t('Pure water'));
    if(tool.VolumeL>0)html+=row('Temperature','25 °C · '+t('Fixed model'));
    if(reading.ph!=null)html+=row('pH',num(reading.ph));
    const targets=current.Tools.filter(s=>s.Id!==tool.Id&&fluid(s)&&s.VolumeL<s.CapacityL&&!(s.Moles>0&&tool.Moles>0&&s.SoluteId!==tool.SoluteId));
    if(tool.VolumeL>0&&(tool.Kind!=='volpipette'||Math.abs(tool.VolumeL-.025)<1e-9)){
      html+=field('Transfer to',`<select id="transfer-target">${targets.map(s=>`<option value="${s.Id}">${escape(identified(s))}</option>`).join('')}</select>`);
      html+=field('Transfer volume (mL)',`<input id="transfer-volume" type="number" required min="0.1" max="${Math.min(500,tool.VolumeL*1000)}" step="0.1" value="${Math.min(25,tool.VolumeL*1000)}" style="width:100%"><button class="primary block" id="transfer" ${!targets.length?'disabled':''}>${escape(t('Transfer solution'))}</button>`);
    }
    const sources=current.Tools.filter(s=>s.Id!==tool.Id&&fluid(s)&&s.VolumeL>0&&!(tool.Moles>0&&s.Moles>0&&tool.SoluteId!==s.SoluteId)&&(s.Kind!=='volpipette'||Math.abs(s.VolumeL-.025)<1e-9&&tool.CapacityL-tool.VolumeL>=.025));
    if(tool.VolumeL<tool.CapacityL&&sources.length)html+=`<div class="fill-controls">${field('Available source',`<select id="fill-source">${sources.map(s=>`<option value="${s.Id}">${escape(identified(s))} · ${num(s.VolumeL*1000)} mL</option>`).join('')}</select>`)}${field('Fill volume (mL)',`<div class="input-row"><input id="fill-volume" type="number" required min="0.1" max="${Math.min((tool.CapacityL-tool.VolumeL)*1000,sources[0].VolumeL*1000)}" step="0.1" value="${Math.min(25,(tool.CapacityL-tool.VolumeL)*1000,sources[0].VolumeL*1000)}"><button id="fill" class="primary">${escape(t('Fill'))}</button></div>`)}</div>`;
    if(tool.VolumeL>0)html+=`<button id="empty-vessel" class="block danger">${escape(t('Empty to waste'))}</button>`;
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
  if(limits){const [min,max,step,label]=limits;html+=field(label,`<div class="input-row"><input id="component-value" required aria-label="${escape(t(label))}" type="number" min="${min}" max="${max}" step="${step}" value="${tool.Value}"><button id="apply-value">${escape(t('Apply'))}</button></div>`);}
  html+=`<div class="inspector-actions"><button id="rotate">↻ ${escape(t('Rotate'))}</button><button id="duplicate">⧉ ${escape(t(fluid(tool)?'Duplicate empty vessel':'Duplicate'))}</button><button id="remove" class="danger">× ${escape(t('Remove'))}</button></div>`;
  html+=`<details style="margin-top:12px"><summary class="muted small">${escape(t('Bench position'))}</summary><div class="input-row" style="margin-top:7px"><label class="small">X<input id="position-x" aria-label="X position" type="number" min="-3" max="3" step=".05" value="${tool.X}"></label><label class="small">Z<input id="position-z" aria-label="Z position" type="number" min="-1.5" max="1.5" step=".05" value="${tool.Z}"></label><button id="apply-position">${escape(t('Apply'))}</button></div></details>`;
  if(!chemical()){
    html+=`<div class="divider"></div>${row('Circuit status',t(circuit?.status||'EmptyCircuit'))}`;
    const terminals=current.Tools.filter(electrical).flatMap(s=>['a','b'].map(end=>({id:s.Id+'.'+end,label:identified(s)+' · '+(end==='a'?'A (+)':'B (−)')})));
    const options=terminals.map(s=>`<option value="${s.id}">${escape(s.label)}</option>`).join('');
    html+=`<p class="terminal-hint">${escape(t('Click two colored terminals, or use the terminal selectors.'))} ${language==='ar'?'اسحب طرفاً لتوصيله؛ اضغط Alt واسحب طرفاً موصولاً لنقله.':'Drag a terminal to connect; Alt-drag a connected terminal to move a lead end.'}</p><div class="connection-row"><select id="terminal-a" aria-label="First terminal">${options}</select><select id="terminal-b" aria-label="Second terminal">${options}</select></div><button id="add-wire" class="block">+ ${escape(t('Add lead'))}</button>${electrical(tool)&&current.Wires.some(w=>w.A.startsWith(tool.Id+'.')||w.B.startsWith(tool.Id+'.'))?`<button id="disconnect-tool" class="block">${escape(t('Disconnect'))}</button>`:''}<label class="field-label">${escape(t('Connections'))} (${current.Wires.length})</label><div class="wire-list">${current.Wires.length?current.Wires.map(w=>`<div class="wire-row"><span>${escape(shortTerminal(w.A)+' ↔ '+shortTerminal(w.B))}</span><button class="wire-end-button" data-rewire-a="${w.Id}" title="Move end A">A↗</button><button class="wire-end-button" data-rewire-b="${w.Id}" title="Move end B">B↗</button><button data-unwire="${w.Id}" aria-label="Disconnect lead">×</button></div>`).join(''):`<span class="muted small">${escape(t('No leads yet.'))}</span>`}</div>`;
  }
  $('inspector').innerHTML=html;
  if($('terminal-b'))$('terminal-b').selectedIndex=Math.min(1,$('terminal-b').options.length-1);
  if(paused)$('inspector').querySelectorAll('button:not([data-allow-paused]),input,select:not(#selected-tool)').forEach(control=>control.disabled=true);
}
function shortTerminal(id){const [toolId,end]=id.split('.');const tool=state().Tools.find(s=>s.Id===toolId);return tool?identified(tool)+'.'+end:id;}
function render(){
  document.body.dataset.lab=workspace.ActiveExperiment;
  for(const button of document.querySelectorAll('button[data-lab]'))button.setAttribute('aria-selected',String(button.dataset.lab===workspace.ActiveExperiment));
  $('presets').hidden=chemical();$('scene-title').textContent=chemical()?(language==='ar'?'تخفيف المحاليل':'SOLUTION DILUTION'):(language==='ar'?'مختبر التيار المستمر':'DC CIRCUIT EXPLORER');
  $('undo').disabled=(chemical()?workspace.ChemistryUndo:workspace.ElectricityUndo).length===0;
  $('redo').disabled=redo[workspace.ActiveExperiment].length===0;
  renderLibrary();renderInspector();scene?.update(state(),readings,catalog.tools,catalog.elements,language);renderContext();guide?.render();renderWirePending();
  if(selectedElement)renderElementDetail();
}
function renderWirePending(){
  $('wire-pending').hidden=!wireStart;$('cancel-wire').hidden=!wireStart;
  if(wireStart)$('wire-pending').textContent=shortTerminal(wireStart)+' · '+t('Click a second terminal to complete the lead.');
  scene?.setWireStart(wireStart,rewiring);
}
function beginRewire(id,end){const wire=state().Wires.find(w=>w.Id===id);if(!wire||paused)return;rewiring={id,end};wireStart=end==='A'?wire.B:wire.A;renderWirePending();status(language==='ar'?'اختر الطرف البديل؛ لن يتغير السلك حتى التأكيد.':'Choose the replacement terminal; the lead stays unchanged until committed.');}
async function terminal(id){if(busy||paused)return;if(!wireStart){wireStart=id;renderWirePending();return;}const a=wireStart,edit=rewiring;wireStart=null;rewiring=null;renderWirePending();if(a!==id)await run(edit?'rewire':'wire',edit?{Content:edit.id,[edit.end]:id}:{A:a,B:id});}
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
  $('help-content').insertAdjacentHTML('beforeend',language==='ar'?'<h3>الدليل والأدوات السريعة</h3><p>افتح الوضع الموجّه واختر تجربة. تتحقق الخطوات من الحالة العلمية الحقيقية، ولا تُستكمل بالنقر. استخدم التلميحات المتدرجة أو عرض الحل، وسجّل القياس لإكمال التجربة. يحفظ الدفتر المحاولات والخطوة الحالية والملاحظات ومنضدة كل تجربة غير مكتملة. الدروس المفاهيمية لا تُحتسب تجارب محاكاة.</p><p>اسحب بطاقة أداة إلى العلامة الخضراء؛ يُرفض التداخل. اسحب طرفاً لتمديد سلك، أو اضغط Alt واسحب طرفاً موصولاً لتحريك نهاية السلك. يمكن اختيار النهاية A أو B من التفاصيل. الإفراغ يسجّل إلقاء المحتوى إلى النفايات، ونسخ الوعاء ينشئ نسخة فارغة.</p><p>Ctrl+Z للتراجع، و Ctrl+Y أو Ctrl+Shift+Z للإعادة. الإيقاف المؤقت يقفل التعديل؛ نموذج DC والمحلول هنا في حالة مستقرة ولا يوجد محرك زمني لإيقافه. تصدير الدفتر يحفظ الحالتين والدليل والإعادة في JSON، ويمكن أيضاً استيراد مساحات العمل القديمة.</p>':'<h3>Guides and quick controls</h3><p>Choose Guided Mode and an experiment. Steps validate the actual scientific state; clicks cannot complete them. Use progressive hints or Show solution, then record the measurement to finish. Your notebook saves attempts, current steps, notes and a separate bench for each unfinished experiment. Concept readings do not count as simulated experiments.</p><p>Drag a library card onto a green placement preview; overlapping drops are rejected. Drag a terminal to extend a wire, or Alt-drag a connected terminal to move a lead end. You can also choose endpoint A or B in Details. Empty records discarded contents as waste; duplicating a container creates an empty copy.</p><p>Ctrl+Z undoes; Ctrl+Y or Ctrl+Shift+Z redoes. Pause locks editing: these are steady-state solution/DC models without a time integrator to pause. Export notebook saves both benches, guide progress and redo in JSON. Legacy workspace files can still be imported.</p>');
}

function bind(){
  $('language').onclick=()=>{language=language==='ar'?'en':'ar';try{localStorage.setItem('virtual-stem-language',language);}catch{}setLanguage();render();renderElements();if($('save-state').textContent)$('save-state').textContent=t(storageSaved?'Saved in this browser':'Save unavailable; export your draft to keep it.');status(t('Ready to explore.'));};
  $('equipment-search').oninput=()=>renderLibrary();$('category').onchange=()=>renderLibrary();
  $('equipment-list').onclick=async event=>{const button=event.target.closest('[data-add]');if(button&&await run('add',{Kind:button.dataset.add,Content:button.dataset.content||null,...guidedPlacement(button.dataset.add,button.dataset.content)})&&innerWidth<=720)panel('library',false);};
  $('equipment-list').ondragstart=event=>{const button=event.target.closest('[data-add]');if(!button||paused||busy||button.disabled){event.preventDefault();return;}event.dataTransfer.setData('application/x-virtual-stem-tool',JSON.stringify({kind:button.dataset.add,content:button.dataset.content||null}));event.dataTransfer.effectAllowed='copy';scene?.showPlacement();};
  $('equipment-list').ondragend=()=>{scene?.clearPlacement();$('drop-notice').hidden=true;};
  const stage=$('viewport');
  stage.ondragover=event=>{if(paused||!event.dataTransfer.types.includes('application/x-virtual-stem-tool'))return;event.preventDefault();const p=scene?.previewPlacement(event.clientX,event.clientY);event.dataTransfer.dropEffect=p?.valid?'copy':'none';$('drop-notice').hidden=false;$('drop-notice').classList.toggle('invalid',!p?.valid);$('drop-notice').textContent=p?.valid?(language==='ar'?'أفلت لوضع الأداة على المنضدة.':'Drop to place on the workbench.'):(language==='ar'?'حرّكها إلى موضع خالٍ داخل المنضدة.':'Move to an unoccupied position on the bench.');};
  stage.ondragenter=stage.ondragover;
  stage.ondragleave=event=>{if(!stage.contains(event.relatedTarget)){$('drop-notice').hidden=true;scene?.clearPlacement();}};
  stage.ondrop=async event=>{event.preventDefault();$('drop-notice').hidden=true;const p=scene?.previewPlacement(event.clientX,event.clientY);scene?.clearPlacement();if(!p?.valid||paused)return;try{const data=JSON.parse(event.dataTransfer.getData('application/x-virtual-stem-tool'));await run('add',{Kind:data.kind,Content:data.content,X:p.x,Z:p.z});}catch(error){status(error.message,true);}};
  for(const button of document.querySelectorAll('button[data-lab]'))button.onclick=()=>switchLab(button.dataset.lab);
  $('library-toggle').onclick=()=>panel('library',document.body.classList.contains('library-collapsed'));
  $('inspector-toggle').onclick=()=>panel('inspector',document.body.classList.contains('inspector-collapsed'));
  $('library-close').onclick=()=>panel('library',false);$('inspector-close').onclick=()=>panel('inspector',false);
  $('undo').onclick=()=>{wireStart=null;rewiring=null;run('undo');};$('redo').onclick=()=>{wireStart=null;rewiring=null;run('redo');};
  $('reset').onclick=async()=>{if(guide?.active()){await guide.confirmStart(guide.current().id);return;}if(await confirmAction(language==='ar'?'إعادة تعيين المنضدة؟':'Reset this workbench?',language==='ar'?'ستُستعاد الأدوات المبدئية. يُحفظ المختبر الآخر، ويمكن التراجع عن إعادة التعيين.':'The default tools will replace this bench. The other lab is preserved, and Reset can be undone.')){wireStart=null;rewiring=null;await run('reset');}};
  $('context-toolbar').onclick=event=>{const button=event.target.closest('[data-context]');if(button)contextual(button.dataset.context);};
  $('pause').onclick=()=>setPaused(!paused);$('resume-editing').onclick=()=>setPaused(false);
  $('select-mode').onclick=()=>{for(const id of ['select-mode','move-mode']){$(id).classList.toggle('active',id==='select-mode');$(id).setAttribute('aria-pressed',String(id==='select-mode'));}scene?.setMoveMode(false);};
  $('move-mode').onclick=()=>{for(const id of ['select-mode','move-mode']){$(id).classList.toggle('active',id==='move-mode');$(id).setAttribute('aria-pressed',String(id==='move-mode'));}scene?.setMoveMode(true);};
  $('measure-quick').onclick=()=>contextual('measure');$('save-local').onclick=()=>{save();status(storageSaved?(language==='ar'?'تم حفظ التجربة والدليل محلياً.':'Experiment and guide saved locally.'):t('Save unavailable; export your draft to keep it.'),!storageSaved);};
  $('camera-reset').onclick=()=>scene?.resetCamera();$('quality').onclick=()=>{lowQuality=!lowQuality;scene?.quality(lowQuality);status(t(lowQuality?'Graphics: low':'Graphics: realistic'));};
  $('cancel-wire').onclick=()=>{wireStart=null;rewiring=null;renderWirePending();};
  $('elements').onclick=()=>{renderElements();$('periodic-dialog').showModal();};$('presets').onclick=openPresets;$('help').onclick=showHelp;
  document.querySelectorAll('.close-dialog').forEach(button=>button.onclick=()=>button.closest('dialog').close());
  for(const dialog of document.querySelectorAll('dialog'))dialog.addEventListener('click',event=>{if(event.target===dialog){const r=dialog.getBoundingClientRect();if(event.clientX<r.left||event.clientX>r.right||event.clientY<r.top||event.clientY>r.bottom)dialog.close();}});
  $('element-search').oninput=filterElements;$('periodic-grid').onclick=event=>{const button=event.target.closest('[data-element]');if(button){selectedElement=catalog.elements.find(e=>e.Symbol===button.dataset.element);renderElements();}};
  $('place-element').onclick=async()=>{if(await run('add',{Kind:'element',Content:selectedElement.Symbol,...guidedPlacement('element',selectedElement.Symbol)}))$('periodic-dialog').close();};
  $('preset-list').onclick=async event=>{const button=event.target.closest('[data-preset]');if(button&&await run('preset',{Content:button.dataset.preset})){$('preset-dialog').close();wireStart=null;renderWirePending();}};
  $('inspector').onclick=async event=>{
    const tool=state().Tools.find(s=>s.Id===state().SelectedId);if(!tool)return;
    const button=event.target.closest('button');if(!button)return;
    const fields={Tool:tool.Id};
    if(button.dataset.logic!=null){const bits=tool.Mode.split(''),bit=Number(button.dataset.logic);bits[bit]=bits[bit]==='1'?'0':'1';await run('mode',{...fields,Content:bits.join('')});}
    else if(button.dataset.unwire)await run('unwire',{Content:button.dataset.unwire});
    else if(button.dataset.rewireA)beginRewire(button.dataset.rewireA,'A');
    else if(button.dataset.rewireB)beginRewire(button.dataset.rewireB,'B');
    else if(button.id==='apply-value'){const input=$('component-value');if(input.reportValidity())await run('value',{...fields,Value:Number(input.value)});}
    else if(button.id==='transfer'){const input=$('transfer-volume');if(input.reportValidity())await run('transfer',{...fields,Target:$('transfer-target').value,Value:Number(input.value)});}
    else if(button.id==='toggle-switch')await run('mode',{...fields,Content:tool.Mode==='closed'?'open':'closed'});
    else if(button.id==='rotate')await run('rotate',fields);
    else if(button.id==='duplicate')await run('duplicate',fields);
    else if(button.id==='empty-vessel')await run('empty',fields);
    else if(button.id==='fill'){const input=$('fill-volume');if(input.reportValidity())await run('transfer',{Tool:$('fill-source').value,Target:tool.Id,Value:Number(input.value)});}
    else if(button.id==='disconnect-tool')await run('disconnect',fields);
    else if(button.id==='remove'){wireStart=null;await run('remove',fields);}
    else if(button.id==='apply-position'){if($('position-x').reportValidity()&&$('position-z').reportValidity())await run('move',{...fields,X:Number($('position-x').value),Z:Number($('position-z').value)});}
    else if(button.id==='add-wire'){wireStart=null;await run('wire',{A:$('terminal-a').value,B:$('terminal-b').value});}
  };
  $('inspector').onchange=event=>{
    const input=event.target,tool=state().Tools.find(s=>s.Id===state().SelectedId);
    if(input.id==='selected-tool'){selectTool(input.value);return;}
    if(input.id==='measurement-target'&&input.value&&tool)run('target',{Tool:tool.Id,Target:input.value});
    if(input.id==='meter-mode'&&tool)run('mode',{Tool:tool.Id,Content:input.value});
    if(input.id==='fill-source'&&tool){const source=state().Tools.find(s=>s.Id===input.value);$('fill-volume').max=Math.min((tool.CapacityL-tool.VolumeL)*1000,source.VolumeL*1000);$('fill-volume').value=Math.min(Number($('fill-volume').value),Number($('fill-volume').max));}
  };
  $('export').onclick=()=>{const url=URL.createObjectURL(new Blob([JSON.stringify(notebook(),null,2)],{type:'application/json'}));const link=document.createElement('a');link.href=url;link.download='virtual-stem-notebook.json';link.click();setTimeout(()=>URL.revokeObjectURL(url),2000);status(t('Draft exported.'));};
  $('import').onclick=()=>$('import-file').click();$('import-file').onchange=async event=>{
    const file=event.target.files[0];event.target.value='';if(!file||busy)return;
    if(file.size>4*1024*1024){status('Draft is too large (maximum 4 MiB).',true);return;}
    busy=true;
    try{const candidate=JSON.parse(await file.text()),isNotebook=candidate.format==='virtual-stem-notebook-v1',result=await request('view',{},isNotebook?candidate.workspace:candidate);workspace=result.workspace;readings=result.readings;circuit=result.circuit;wireStart=null;rewiring=null;redo=cleanRedo(isNotebook?candidate.redo:null);guide.data=cleanLearning(isNotebook?candidate.learning:null);guide.afterChange();save();render();status(t('Draft imported.'));}
    catch(error){status('Draft could not be imported: '+error.message,true);}finally{busy=false;}
  };
  document.addEventListener('keydown',event=>{if(event.key==='Escape'){wireStart=null;rewiring=null;renderWirePending();}if((event.ctrlKey||event.metaKey)&&['z','y'].includes(event.key.toLowerCase())&&!['INPUT','SELECT','TEXTAREA'].includes(event.target.tagName)&&!document.querySelector('dialog[open]')){event.preventDefault();run(event.key.toLowerCase()==='y'||event.shiftKey?'redo':'undo');}});
}
async function start(){
  setLanguage();
  const responses=await Promise.all([fetch('/api/catalog'),fetch('/api/workspace')]);
  if(responses.some(r=>!r.ok))throw new Error('The local C# server is not ready. Start it with python3 tools/browser_lab.py.');
  const [data,initial]=await Promise.all(responses.map(r=>r.json()));catalog=data;workspace=initial.workspace;readings=initial.readings;circuit=initial.circuit;
  let restoreError,learning;
  try{const draft=localStorage.getItem(NOTEBOOK)||localStorage.getItem(KEY);if(draft){const stored=JSON.parse(draft),isNotebook=stored.format==='virtual-stem-notebook-v1',result=await request('view',{},isNotebook?stored.workspace:stored);workspace=result.workspace;readings=result.readings;circuit=result.circuit;learning=isNotebook?stored.learning:null;redo=cleanRedo(isNotebook?stored.redo:null);restored=true;}}
  catch(error){restoreError=error.message;}
  await document.fonts.ready;
  try{scene=new LabScene($('viewport'),{select:selectTool,terminal,beginWire:id=>{if(!paused){wireStart=id;rewiring=null;renderWirePending();}},beginRewire,frame:positionContext,move:(id,x,z)=>run('move',{Tool:id,X:x,Z:z}),notice:message=>status(message,true)});}
  catch(error){$('viewport').innerHTML='<div class="loading"><p>WebGL 2 is unavailable. Enable hardware acceleration and reload.</p><p class="small">The equipment controls and calculated readings remain available.</p></div>';console.error(error);}
  guide=new Guide({lang:()=>language,translate:t,context:()=>lessonContext(workspace,readings,circuit),toolLabel:kind=>kind==='meter'?(language==='ar'?'مجس / ملتيميتر':'Probe / multimeter'):definition(kind)?.[language==='ar'?'Arabic':'English']||kind,run,switchLab,save,render,focusStep,highlight:highlightStep,confirm:confirmAction,resumeEditing:()=>setPaused(false)},learning);
  selectedElement=catalog.elements.find(e=>e.Symbol==='Cu');bind();guide.afterChange();render();if(scene)$('loading')?.remove();
  status(restoreError?'Saved draft could not be restored. It remains in browser storage until your next successful edit. '+restoreError:restored?(language==='ar'?'تم استئناف التجربة المحفوظة.':'Saved workspace resumed.'):t('Ready to explore.'));
  // Read-only diagnostics support the browser smoke harness without bypassing UI commands.
  window.labDiagnostics={get workspace(){return structuredClone(workspace);},get readings(){return structuredClone(readings);},get circuit(){return structuredClone(circuit);},get learning(){return structuredClone(guide.data);},get redo(){return structuredClone(redo);},get renderedTools(){return scene?.groups.size||0;},project:(id,end)=>scene?.projected(id,end),projectBench:(x,z)=>scene?.projectBench(x,z),get renderer(){return scene?.renderer.info.render;}};
  document.body.dataset.ready='true';
}
start().catch(error=>{status(error.message,true);$('loading').innerHTML=`<p>Could not start the laboratory.</p><p class="small">${escape(error.message)}</p>`;console.error(error);});
