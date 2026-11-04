import test from 'node:test';
import assert from 'node:assert/strict';
import {lessons,context,evaluateStep,evaluateResult,tool} from '../../src/BrowserLab/wwwroot/experiments.mjs';
import {Guide,cleanLearning} from '../../src/BrowserLab/wwwroot/guide.mjs';

const base=process.argv.find(a=>a.startsWith('http'))||process.env.STEM_BROWSER_URL||'http://127.0.0.1:5080';
async function request(path,data){const response=await fetch(base+path,data?{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(data)}:{});const result=await response.json();assert.equal(response.status,200,JSON.stringify(result));return result;}
async function fixture(lesson){
  let result=await request('/api/workspace');result.workspace.ActiveExperiment=lesson.lab;
  const command=async(Action,fields={})=>{result=await request('/api/command',{Action,Workspace:result.workspace,...fields});return result;};
  await command(lesson.setup?'preset':'clear',lesson.setup?{Content:lesson.setup}:{});
  const c=()=>context(result.workspace,result.readings,result.circuit);
  const get=(kind,index=0,content)=>tool(c(),kind,index,content);
  const controller=Object.create(Guide.prototype);controller.data=cleanLearning({activeId:lesson.id,guided:true,progress:{[lesson.id]:{attempts:1}}});controller.host={context:c};
  const wire=async(ka,ea,kb,eb,ia=0,ib=0)=>command('wire',{A:get(ka,ia).Id+'.'+ea,B:get(kb,ib).Id+'.'+eb});
  const target=async(kind,other,index=0,content)=>command('target',{Tool:get(kind).Id,Target:get(other,index,content).Id});
  return {command,c,get,wire,target,controller};
}

test('Curriculum has complete bilingual metadata and separates runnable from conceptual lessons',()=>{
  assert.equal(lessons.length,18);assert.equal(lessons.filter(l=>l.type==='hands-on').length,12);assert.equal(new Set(lessons.map(l=>l.id)).size,18);
  for(const lesson of lessons){
    for(const field of ['title','objective','materials','safety','observations','explanation','mistakes'])for(const language of ['en','ar'])assert.ok(lesson[field]?.[language]?.length>8,lesson.id+' '+field+' '+language);
    assert.ok(lesson.duration>0&&lesson.equipment.length>0&&lesson.questions.length>0&&lesson.equation);
    if(lesson.type==='hands-on'){assert.ok(lesson.steps.length>=3);for(const s of lesson.steps){assert.equal(s.hints.length,3);assert.ok(s.instruction.en&&s.instruction.ar);}assert.ok(Number.isFinite(lesson.expected));}
    else {assert.ok(lesson.reason.en&&lesson.reason.ar&&lesson.procedure.length>=3);assert.equal(lesson.steps.length,0);}
  }
});

for(const lesson of lessons.filter(l=>l.type==='hands-on'))test('Real scientific workflow: '+lesson.id,async()=>{
  const f=await fixture(lesson),{command,c,get,wire,target,controller}=f;
  if(!lesson.setup)assert.equal(evaluateResult(lesson,c()).valid,false,'An empty bench cannot pass');
  assert.equal(controller.progress().finished,false,'A prepared setup does not complete a guided attempt');
  for(let i=0;i<lesson.steps.length;i++){
    const s=lesson.steps[i];
    if(s.focus==='library')await command('add',{Kind:s.kind,Content:s.content,X:s.placement[0],Z:s.placement[1]});
    else if(s.optional){assert.equal(evaluateStep(lesson,i,c()),false);controller.progress().checks.push({step:i,skipped:true,observed:null,revision:c().state.Revision});controller.progress().cursor=i+1;continue;}
    else if(s.record){
      assert.equal(evaluateStep(lesson,i,c(),s.record.expected+10),false,'Wrong student record does not advance');
      controller.progress().entries[i]=s.record.expected;
    }else switch(lesson.id+':'+i){
      case 'volume:2':await command('transfer',{Tool:get('water').Id,Target:get('cylinder').Id,Value:25});break;
      case 'dilution:3':await command('transfer',{Tool:get('stock').Id,Target:get('beaker').Id,Value:25});break;
      case 'dilution:4':await command('transfer',{Tool:get('water').Id,Target:get('beaker').Id,Value:75});break;
      case 'dilution:6':await target('meter','beaker');break;
      case 'ph:4':await target('phmeter','stock',0,'HCl');break;
      case 'ph:5':await command('move',{Tool:get('phmeter').Id,X:0,Z:1.1});await target('phmeter','water');break;
      case 'ph:6':await command('move',{Tool:get('phmeter').Id,X:1.8,Z:1.1});await target('phmeter','stock',0,'NaOH');break;
      case 'sample-mass:2':await target('balance','element');break;
      case 'sample-mass:3':await command('value',{Tool:get('element').Id,Value:.02});break;
      case 'led:3':await wire('battery','a','resistor','a');await wire('resistor','b','led','a');await wire('led','b','battery','b');break;
      case 'ohms-law:2':await wire('battery','a','resistor','a');await wire('resistor','b','battery','b');break;
      case 'ohms-law:3':await command('value',{Tool:get('battery').Id,Value:6});break;
      case 'ohms-law:4':await command('value',{Tool:get('battery').Id,Value:9});break;
      case 'series:3':await wire('battery','a','resistor','a');await wire('resistor','b','resistor','a',0,1);await wire('resistor','b','battery','b',1);break;
      case 'parallel:3':await wire('battery','a','resistor','a');await wire('resistor','b','battery','b');await wire('battery','a','resistor','a',0,1);await wire('resistor','b','battery','b',1);break;
      case 'meters:3':await wire('battery','a','resistor','a');await wire('resistor','b','battery','b');await wire('meter','a','resistor','a');await wire('meter','b','resistor','b');break;
      case 'meters:4':
        for(const lead of [...c().state.Wires])await command('unwire',{Content:lead.Id});
        await command('mode',{Tool:get('meter').Id,Content:'current'});await wire('battery','a','meter','a');await wire('meter','b','resistor','a');await wire('resistor','b','battery','b');break;
      case 'led-polarity:1':await command('disconnect',{Tool:get('led').Id});await wire('resistor','b','led','b');await wire('led','a','battery','b');break;
      case 'switch:0':await command('mode',{Tool:get('switch').Id,Content:'open'});break;
      case 'switch:1':await command('mode',{Tool:get('switch').Id,Content:'closed'});break;
      case 'capacitor-dc:1':await command('value',{Tool:get('capacitor').Id,Value:.0002});break;
    }
    assert.equal(evaluateStep(lesson,i,c(),s.record?.expected),true,lesson.id+' step '+i+' must be achievable with the actual engine');
    controller.afterChange();
  }
  const result=evaluateResult(lesson,c());assert.equal(result.valid,true,JSON.stringify(result));
  assert.equal(controller.progress().finished,true);assert.equal(controller.progress().completions,1);
  assert.equal(controller.progress().cursor,lesson.steps.length);
  const restored=cleanLearning(JSON.parse(JSON.stringify(controller.data)));assert.equal(restored.progress[lesson.id].completions,1);assert.ok(restored.progress[lesson.id].checks.length>=lesson.steps.length-1);
  // A different physical state must not pass just because the lesson cursor was persisted as complete.
  await command('clear');assert.equal(evaluateResult(lesson,c()).valid,false);
});

test('Required steps never advance on clicks, supplied answers or hints without the scientific setup',async()=>{
  const lesson=lessons.find(l=>l.id==='volume'),{controller,c}=await fixture(lesson);
  controller.progress().entries[3]=25;
  for(let click=0;click<20;click++){controller.progress().hint=3;controller.afterChange();}
  assert.equal(controller.progress().cursor,0);assert.equal(controller.progress().completions,0);
  assert.equal(evaluateStep(lesson,3,c(),25),false);
});

test('Concept readings cannot produce a simulated experimental result',async()=>{
  const {c}=await fixture(lessons.find(l=>l.id==='volume'));
  for(const lesson of lessons.filter(l=>l.type==='concept')){assert.equal(evaluateStep(lesson,0,c(),100),false);assert.equal(evaluateResult(lesson,c()).valid,false);}
});

test('Persisted learning data is bounded and an unfinished attempt can resume',()=>{
  const state=cleanLearning({activeId:'volume',guided:true,progress:{volume:{attempts:2,cursor:2,entries:{3:25},notes:'x'.repeat(5000)},unknown:{cursor:99999}}});
  assert.equal(state.activeId,'volume');assert.equal(state.progress.volume.cursor,2);assert.equal(state.progress.volume.attempts,2);assert.equal(state.progress.volume.notes.length,4000);assert.equal(state.progress.unknown,undefined);
});
