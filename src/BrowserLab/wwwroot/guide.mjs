import {lessons,findLesson,text,evaluateStep,evaluateResult} from './experiments.mjs';

const $=id=>document.getElementById(id);
const esc=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const number=value=>value==null?'—':Math.abs(value)>0&&Math.abs(value)<.0001?value.toExponential(3):Number(value.toPrecision(6)).toString();
export function cleanLearning(source={}){
  const data={activeId:findLesson(source?.activeId)?.type==='hands-on'?source.activeId:null,guided:source?.guided===true,boardLesson:{chemistry:null,electricity:null},progress:{}};
  for(const lab of ['chemistry','electricity'])if(findLesson(source?.boardLesson?.[lab])?.lab===lab)data.boardLesson[lab]=source.boardLesson[lab];
  for(const lesson of lessons){
    const old=source?.progress?.[lesson.id];if(!old||typeof old!=='object')continue;
    const cap=(value,max)=>Number.isInteger(value)?Math.min(max,Math.max(0,value)):0;
    const entries={};for(const [key,value] of Object.entries(old.entries||{}))if(/^\d+$/.test(key)&&Number(key)<lesson.steps.length&&typeof value==='number'&&Number.isFinite(value))entries[key]=value;
    data.progress[lesson.id]={attempts:cap(old.attempts,10000),completions:cap(old.completions,10000),cursor:cap(old.cursor,lesson.steps.length),hint:cap(old.hint,3),
      entries,checks:Array.isArray(old.checks)?old.checks.slice(0,lesson.steps.length).map(c=>({step:cap(c?.step,lesson.steps.length),observed:typeof c?.observed==='number'&&Number.isFinite(c.observed)?c.observed:null,revision:cap(c?.revision,Number.MAX_SAFE_INTEGER),skipped:c?.skipped===true})):[],
      finished:old.finished===true,readAt:typeof old.readAt==='string'?old.readAt.slice(0,40):null,best:typeof old.best==='number'&&Number.isFinite(old.best)?old.best:null,
      notes:typeof old.notes==='string'?old.notes.slice(0,4000):'',lastResult:old.lastResult&&Number.isFinite(old.lastResult.observed)?{observed:old.lastResult.observed,expected:lesson.expected,difference:old.lastResult.observed-lesson.expected}:null,
      checkpoint:old.checkpoint?.Experiment===lesson.lab&&Array.isArray(old.checkpoint.Tools)&&old.checkpoint.Tools.length<=12&&Array.isArray(old.checkpoint.Wires)&&old.checkpoint.Wires.length<=64&&Array.isArray(old.checkpoint.Actions)&&old.checkpoint.Actions.length<=100?structuredClone(old.checkpoint):null};
    const p=data.progress[lesson.id];
    p.finished=lesson.type==='hands-on'&&p.finished&&p.cursor===lesson.steps.length&&p.completions>0&&p.lastResult!==null&&Math.abs(p.lastResult.difference)<=(lesson.tolerance??1e-8)&&lesson.steps.every((s,i)=>s.optional||p.checks.some(check=>check.step===i&&!check.skipped));
    if(lesson.type==='hands-on'&&p.cursor===lesson.steps.length&&!p.finished)p.cursor=lesson.steps.length-1;
  }
  return data;
}
export class Guide {
  constructor(host,data){this.host=host;this.data=cleanLearning(data);this.filterDetail=null;this.bind();}
  label(en,ar){return this.host.lang()==='ar'?ar:en;}
  current(){return findLesson(this.data.activeId);}
  progress(lesson=this.current()){
    if(!lesson)return null;
    return this.data.progress[lesson.id]??(this.data.progress[lesson.id]={attempts:0,completions:0,cursor:0,hint:0,entries:{},checks:[],finished:false,readAt:null,best:null,notes:'',lastResult:null});
  }
  step(){const lesson=this.current();return this.data.guided&&lesson?.lab===this.host.context().state.Experiment?lesson.steps[this.progress().cursor]:null;}
  active(){return this.data.guided&&this.current()?.lab===this.host.context().state.Experiment;}
  afterChange(action='view'){
    const c=this.host.context(),boardId=this.data.boardLesson?.[c.state.Experiment];
    if(['reset','clear','preset'].includes(action)){
      if(boardId)this.data.guided=false;
      if(this.data.boardLesson)this.data.boardLesson[c.state.Experiment]=null;
    }else if(boardId&&this.data.progress[boardId])this.data.progress[boardId].checkpoint=structuredClone(c.state);
    if(!this.active())return;
    const lesson=this.current(),progress=this.progress();if(progress.finished)return;
    if(action==='undo'){
      const reverted=progress.checks.filter(check=>check.revision>c.state.Revision);
      if(reverted.length){progress.cursor=Math.min(progress.cursor,...reverted.map(check=>check.step));progress.checks=progress.checks.filter(check=>check.step<progress.cursor);progress.hint=0;}
    }
    let count=0;
    while(progress.cursor<lesson.steps.length&&evaluateStep(lesson,progress.cursor,c,progress.entries[progress.cursor])&&count++<lesson.steps.length){
      const s=lesson.steps[progress.cursor];let observed=null;try{observed=s.observe?.(c)??null;}catch{}
      progress.checks.push({step:progress.cursor,observed:Number.isFinite(observed)?observed:null,revision:c.state.Revision,skipped:false});
      progress.cursor++;progress.hint=0;
    }
    if(progress.cursor===lesson.steps.length){
      const result=evaluateResult(lesson,c);
      if(result.valid){progress.finished=true;progress.completions++;progress.lastResult=result;const error=Math.abs(result.difference);if(progress.best==null||error<progress.best)progress.best=error;}
    }
  }
  async start(id){
    const lesson=findLesson(id);if(!lesson||lesson.type!=='hands-on')return;
    const original=this.data.activeId,wasGuided=this.data.guided;
    this.data.guided=false;this.data.boardLesson[lesson.lab]=null;this.host.resumeEditing();
    if(!await this.host.switchLab(lesson.lab)){this.data.guided=wasGuided;return;}
    if(!await this.host.run(lesson.setup?'preset':'clear',lesson.setup?{Content:lesson.setup}:{})){this.data.activeId=original;this.data.guided=wasGuided;return;}
    const old=this.progress(lesson);
    this.data.progress[id]={...old,attempts:old.attempts+1,cursor:0,hint:0,entries:{},checks:[],finished:false};
    this.data.activeId=id;this.data.guided=true;this.data.boardLesson[lesson.lab]=id;this.filterDetail=null;
    $('guide-dialog').close();this.afterChange();this.host.save();this.host.render();this.useStep();
  }
  async resume(id){
    const lesson=findLesson(id);if(!lesson||lesson.type!=='hands-on')return;
    const previousId=this.data.activeId,previousMode=this.data.guided;this.data.guided=false;
    if(!await this.host.switchLab(lesson.lab)){this.data.activeId=previousId;this.data.guided=previousMode;return;}
    const p=this.progress(lesson);
    if(this.data.boardLesson[lesson.lab]!==id&&p.checkpoint){
      const previousBoard=this.data.boardLesson[lesson.lab];this.data.boardLesson[lesson.lab]=null;
      if(!await this.host.run('restore',{Snapshot:p.checkpoint})){this.data.boardLesson[lesson.lab]=previousBoard;this.data.activeId=previousId;this.data.guided=previousMode;return;}
      // A restored checkpoint starts a new local revision chain; retain the verified observations.
      for(const check of p.checks)check.revision=this.host.context().state.Revision;
    }
    this.data.activeId=id;this.data.guided=true;this.data.boardLesson[lesson.lab]=id;$('guide-dialog').close();this.afterChange();this.host.save();this.host.render();this.useStep();
  }
  useStep(){
    const step=this.step();if(!step)return;
    this.host.focusStep(step);
  }
  browse(){this.filterDetail=null;this.renderBrowser();$('guide-dialog').showModal();}
  renderBrowser(){
    const lang=this.host.lang();
    const guideTitle=$('guide-title');guideTitle.textContent=this.label('Experiments guide','دليل التجارب');
    const query=$('guide-search').value.trim().toLowerCase(),lab=$('guide-lab').value,difficulty=$('guide-difficulty').value,type=$('guide-type').value,duration=Number($('guide-duration').value)||100,topic=$('guide-topic').value;
    const completed=lessons.filter(l=>l.type==='hands-on'&&(this.data.progress[l.id]?.completions||0)>0).length;
    const started=lessons.filter(l=>(this.data.progress[l.id]?.attempts||0)>0).length;
    $('guide-stats').textContent=this.label(`${completed} / 12 hands-on experiments completed · ${started} started`,`${completed} / 12 تجربة عملية مكتملة · ${started} بدأت`);
    const list=lessons.filter(l=>(lab==='all'||l.lab===lab)&&(difficulty==='all'||l.difficulty===difficulty)&&(type==='all'||l.type===type)&&l.duration<=duration&&(topic==='all'||l.topic===topic)&&(!query||`${text(l.title,'en')} ${text(l.title,'ar')} ${text(l.objective,'en')} ${text(l.objective,'ar')} ${l.equipment.map(k=>this.host.toolLabel(k)).join(' ')} ${l.equipment.join(' ')} ${l.topic}`.toLowerCase().includes(query)));
    $('guide-list').innerHTML=list.length?list.map(l=>{
      const p=this.data.progress[l.id],done=l.type==='hands-on'&&p?.completions>0;
      return `<button class="lesson-card ${done?'lesson-done':''}" data-lesson="${l.id}"><span class="lesson-icon">${l.lab==='chemistry'?'⚗':'ϟ'}</span><span class="lesson-card-content"><span class="lesson-meta">${esc(this.host.translate(l.difficulty))} · ${l.duration} ${this.label('min','د')} · ${esc(this.host.translate(l.topic))}</span><strong>${esc(text(l.title,lang))}</strong><span>${esc(text(l.objective,lang))}</span><span class="lesson-type ${l.type==='concept'?'concept':''}">${done?'✓ ':''}${this.label(l.type==='concept'?'Concept lesson — no simulation':done?'Completed':p?.attempts?'In progress':'Hands-on guided lab',l.type==='concept'?'درس مفاهيمي — دون محاكاة':done?'مكتملة':p?.attempts?'قيد التنفيذ':'تجربة عملية موجهة')}</span></span><span class="lesson-arrow">↗</span></button>`;
    }).join(''):`<p class="muted">${this.label('No experiments match these filters.','لا توجد تجارب تطابق المرشحات.')}</p>`;
    if(this.filterDetail)this.showDetail(this.filterDetail);else $('guide-detail').innerHTML=`<div class="guide-welcome"><span>✦</span><h3>${this.label('Learn by doing.','تعلّم بالممارسة.')}</h3><p>${this.label('Choose a lab and an experiment. We will check your setup, guide your measurements and help you explain the result.','اختر مختبراً وتجربة. سنتحقق من إعدادك ونرشد قياساتك ونساعدك على تفسير النتيجة.')}</p><p class="small muted">${this.label('12 runnable experiments · 6 clearly labeled concept lessons','12 تجربة قابلة للتشغيل · 6 دروس مفاهيمية موضحة')}</p></div>`;
  }
  showDetail(id){
    const l=findLesson(id);if(!l)return;this.filterDetail=id;const lang=this.host.lang(),p=this.progress(l);
    const heading=(en,ar)=>`<h4>${this.label(en,ar)}</h4>`;
    $('guide-detail').innerHTML=`<div class="lesson-detail-heading"><span class="lesson-type ${l.type==='concept'?'concept':''}">${this.label(l.type==='hands-on'?'Hands-on guided lab':'Concept lesson',l.type==='hands-on'?'تجربة عملية موجهة':'درس مفاهيمي')}</span><h3>${esc(text(l.title,lang))}</h3><p>${esc(text(l.objective,lang))}</p><div class="lesson-meta">${esc(this.host.translate(l.difficulty))} · ${l.duration} ${this.label('minutes','دقائق')}</div></div>
      ${l.type==='concept'?`<p class="capability-note">${esc(text(l.reason,lang))}</p>`:`<div class="lesson-start-actions"><button class="primary" data-start-lesson="${l.id}">${this.label(p.attempts?'Restart on a clean bench':'Use in Lab',p.attempts?'إعادة البدء بمنضدة فارغة':'استخدم في المختبر')}</button>${p.attempts&&!p.finished?`<button data-resume-lesson="${l.id}">${this.label('Resume current attempt','استئناف المحاولة الحالية')}</button>`:''}</div><p class="small muted">${this.label('Starting replaces this lab’s bench after confirmation. The other lab is preserved.','يستبدل البدء منضدة هذا المختبر بعد التأكيد، ويحافظ على المختبر الآخر.')}</p>`}
      ${heading('Required equipment','الأدوات المطلوبة')}<div class="equipment-chips">${l.equipment.map(k=>`<span>${esc(this.host.toolLabel(k))}</span>`).join('')}</div>${heading('Materials / components','المواد والعناصر')}<p>${esc(text(l.materials,lang))}</p>${heading('Safety and scope','السلامة والنطاق')}<p>${esc(text(l.safety,lang))}</p>
      <details open><summary>${this.label('Step-by-step procedure','الخطوات العملية')}</summary><ol>${(l.type==='hands-on'?l.steps.map(s=>s.instruction):l.procedure).map(s=>`<li>${esc(text(s,lang))}</li>`).join('')}</ol></details>
      <details><summary>${this.label('Expected observations and explanation','الملاحظات المتوقعة والتفسير')}</summary><p>${esc(text(l.observations,lang))}</p><p>${esc(text(l.explanation,lang))}</p><div class="equation" dir="ltr">${esc(l.equation)}</div></details>
      <details><summary>${this.label('Common mistakes and student questions','الأخطاء الشائعة وأسئلة الطالب')}</summary><p>${esc(text(l.mistakes,lang))}</p><ol>${l.questions.map(q=>`<li>${esc(text(q,lang))}</li>`).join('')}</ol></details>
      ${l.source?`<p class="small"><a href="${esc(l.source)}" target="_blank" rel="noopener noreferrer">OpenStax · ${this.label('Scientific reference','المرجع العلمي')}</a></p>`:''}
      ${l.type==='concept'?`<button data-mark-read="${l.id}" class="block">${this.label(p.readAt?'✓ Read — review again':'Mark concept as read',p.readAt?'✓ تمت القراءة — راجع مجدداً':'تسجيل قراءة المفهوم')}</button>`:''}`;
  }
  render(){
    const lesson=this.current(),progress=lesson?this.progress():null,active=this.active();
    $('guide-dock').hidden=!active;$('guide-mode').setAttribute('aria-pressed',String(active));$('free-mode').setAttribute('aria-pressed',String(!active));
    $('guide-resume').hidden=!lesson||active||progress?.finished;
    if(!active){this.host.highlight(null);return;}
    const lang=this.host.lang(),step=lesson.steps[progress.cursor],result=evaluateResult(lesson,this.host.context());
    const title=text(lesson.title,lang),percent=Math.round(progress.cursor/lesson.steps.length*100);
    $('guide-dock').innerHTML=`<div class="guide-dock-heading"><div><span class="eyebrow">${this.label('GUIDED EXPERIMENT','تجربة موجهة')} · ${this.host.translate(lesson.difficulty)}</span><h3>${esc(title)}</h3></div><div class="guide-dock-actions"><button id="guide-collapse" aria-label="${this.label('Collapse guide','طي الدليل')}" title="${this.label('Collapse guide','طي الدليل')}">⌄</button><button data-open-guide title="${this.label('Experiments guide','دليل التجارب')}">☷</button><button data-free-mode title="${this.label('Free Mode','الوضع الحر')}">×</button></div></div>
      <div class="guide-progress"><span style="width:${percent}%"></span></div><div class="guide-body"><div class="guide-step-line"><span>${this.label('Step','الخطوة')} ${Math.min(progress.cursor+1,lesson.steps.length)} / ${lesson.steps.length}</span><span>${percent}% · ${this.label('Attempt','محاولة')} ${progress.attempts}</span></div>
      ${progress.finished?`<div class="guide-complete"><b>✓ ${this.label('Experiment completed','اكتملت التجربة')}</b><p>${this.label('Your scientific state was validated.','تم التحقق من الحالة العلمية.')}</p>${!result.valid?`<p class="capability-note">${this.label('The current bench has changed since completion. Your recorded result is kept.','تغيّرت المنضدة منذ الإنجاز. احتُفظ بالنتيجة المسجلة.')}</p>`:''}<button class="primary block" data-show-result>${this.label('View result & explanation','النتيجة والتفسير')}</button><button class="block" data-start-lesson="${lesson.id}">${this.label('Repeat experiment','تكرار التجربة')}</button></div>`:
        `<p class="guide-instruction">${esc(text(step?.instruction,lang))}</p>${step?.record?`<form id="guide-record-form"><label for="guide-record">${this.label('Your measurement','قياسك')} (${esc(step.record.unit)})</label><div class="input-row"><input id="guide-record" type="number" step="any" required value="${progress.entries[progress.cursor]??''}" aria-label="${this.label('Record measurement','تسجيل القياس')}"><button class="primary" type="submit">${this.label('Check','تحقق')}</button></div><p id="record-feedback" class="small" role="status"></p></form>`:''}
        <div class="guide-help-actions"><button data-use-step>${this.label('Use in Lab','استخدم في المختبر')}</button><button data-guide-hint>${this.label('Hint','تلميح')} ${Math.min(progress.hint+1,2)}</button><button data-guide-solution>${this.label('Show solution','عرض الحل')}</button>${step?.optional?`<button data-guide-skip>${this.label('Skip reflection','تخطّي التأمل')}</button>`:''}</div>${progress.hint?`<div class="guide-hint">${esc(text(step?.hints[Math.min(progress.hint-1,2)],lang))}</div>`:''}<p class="guide-validation-note">${this.label('We check equipment, quantities and calculated readings — not button clicks.','نتحقق من الأدوات والكميات والقراءات المحسوبة، وليس النقرات.')}</p>`}</div>`;
    $('guide-dock').classList.toggle('is-collapsed',this.collapsed===true);
    this.host.highlight(step??null);
  }
  showResult(){
    const lesson=this.current(),progress=this.progress(),result=progress.lastResult??evaluateResult(lesson,this.host.context()),lang=this.host.lang();
    $('result-content').innerHTML=`<span class="eyebrow">${this.label('SCIENTIFIC RESULT','النتيجة العلمية')}</span><h2>${esc(text(lesson.title,lang))}</h2><div class="result-values"><div><span>${this.label('Expected','المتوقع')}</span><b>${number(result.expected)} <small>${esc(lesson.unit)}</small></b></div><div><span>${this.label('Observed in the model','الملاحظ في النموذج')}</span><b>${number(result.observed)} <small>${esc(lesson.unit)}</small></b></div><div><span>${this.label('Difference','الفرق')}</span><b>${number(result.difference)} <small>${esc(lesson.unit)}</small></b></div></div><h3>${this.label('What happened — and why','ماذا حدث، ولماذا')}</h3><p>${esc(text(lesson.explanation,lang))}</p><div class="equation" dir="ltr">${esc(lesson.equation)}</div><h3>${this.label('Your validated steps','خطواتك المتحققة')}</h3><ol class="result-checks">${progress.checks.map(c=>`<li>${c.skipped?this.label('Optional reflection skipped','تُخطي التأمل الاختياري'):esc(text(lesson.steps[c.step]?.instruction,lang))}${c.observed!=null?` <bdi>(${number(c.observed)})</bdi>`:''}</li>`).join('')}</ol><h3>${this.label('Think about it','فكّر في الأمر')}</h3><ol>${lesson.questions.map(q=>`<li>${esc(text(q,lang))}</li>`).join('')}</ol><label class="field-label" for="guide-notes">${this.label('Your notes (saved locally)','ملاحظاتك (محفوظة محلياً)')}</label><textarea id="guide-notes" maxlength="4000" rows="3">${esc(progress.notes)}</textarea><p class="small muted">${this.label(`Completions: ${progress.completions} · Best absolute result difference: ${number(progress.best)} ${lesson.unit}`,`الإنجازات: ${progress.completions} · أفضل فرق مطلق: ${number(progress.best)} ${lesson.unit}`)}</p><button class="primary" data-start-lesson="${lesson.id}">${this.label('Repeat experiment','تكرار التجربة')}</button>`;
    $('result-dialog').showModal();
  }
  async confirmStart(id){
    const lesson=findLesson(id);if(!lesson)return;
    if(this.host.context().state.Tools.length>0){
      if(!await this.host.confirm(this.label('Start a new guided experiment?','بدء تجربة موجهة جديدة؟'),this.label('This replaces the selected lab’s bench. The action is undoable; save or export first if you want a separate copy.','سيستبدل هذا منضدة المختبر المختار. يمكنك التراجع؛ احفظ أو صدّر أولاً إن أردت نسخة منفصلة.')))return;
    }
    $('result-dialog').close();await this.start(id);
  }
  bind(){
    for(const id of ['guide-search','guide-lab','guide-difficulty','guide-type','guide-duration','guide-topic'])$(id).addEventListener(id==='guide-search'?'input':'change',()=>{this.filterDetail=null;this.renderBrowser();});
    document.addEventListener('click',async event=>{
      const b=event.target.closest('button');if(!b)return;
      if(b.dataset.lesson)this.showDetail(b.dataset.lesson);
      if(b.hasAttribute('data-open-guide')||b.id==='guide-mode')this.browse();
      if(b.dataset.startLesson)await this.confirmStart(b.dataset.startLesson);
      if(b.dataset.resumeLesson)await this.resume(b.dataset.resumeLesson);
      if(b.hasAttribute('data-free-mode')||b.id==='free-mode'){this.data.guided=false;this.host.save();this.host.render();}
      if(b.id==='guide-resume')await this.resume(this.current().id);
      if(b.hasAttribute('data-use-step'))this.useStep();
      if(b.hasAttribute('data-guide-hint')||b.hasAttribute('data-guide-solution')){const p=this.progress();p.hint=b.hasAttribute('data-guide-solution')?3:Math.min(3,p.hint+1);this.host.save();this.render();}
      if(b.hasAttribute('data-guide-skip')&&this.step()?.optional){const p=this.progress();p.checks.push({step:p.cursor,observed:null,revision:this.host.context().state.Revision,skipped:true});p.cursor++;p.hint=0;this.afterChange();this.host.save();this.host.render();}
      if(b.hasAttribute('data-show-result'))this.showResult();
      if(b.dataset.markRead){const p=this.progress(findLesson(b.dataset.markRead));p.readAt=new Date().toISOString();this.host.save();this.showDetail(b.dataset.markRead);this.renderBrowser();}
      if(b.id==='guide-collapse'){this.collapsed=!this.collapsed;this.render();}
    });
    document.addEventListener('submit',event=>{
      if(event.target.id!=='guide-record-form')return;event.preventDefault();const input=$('guide-record');if(!input.reportValidity())return;
      const p=this.progress(),before=p.cursor;p.entries[p.cursor]=Number(input.value);this.afterChange();this.host.save();
      if(p.cursor===before)$('record-feedback').textContent=this.label('Not yet. Check the actual measurement, units and required setup.','ليس بعد. راجع القياس الفعلي والوحدات والإعداد المطلوب.');else this.host.render();
    });
    document.addEventListener('input',event=>{if(event.target.id==='guide-notes'){this.progress().notes=event.target.value.slice(0,4000);this.host.save();}});
  }
}
