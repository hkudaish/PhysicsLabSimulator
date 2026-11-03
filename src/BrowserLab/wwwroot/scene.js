import * as THREE from 'three';
import { OrbitControls } from './vendor/OrbitControls.js';
import { HDRLoader } from './vendor/HDRLoader.js';

const BENCH = 0.86;
const colors = { metal:0xb8c6c6, ivory:0xe4e1cf, dark:0x263b38, teal:0x247d77, red:0xc8483f, black:0x273231, copper:0xb66a35 };
export class LabScene {
  constructor(container, callbacks) {
    this.container=container;this.callbacks=callbacks;this.groups=new Map();this.pickables=[];this.ports=new Map();this.dynamicMaterials=new Set();
    this.renderer=new THREE.WebGLRenderer({antialias:true,alpha:false,powerPreference:'high-performance'});
    this.renderer.setPixelRatio(Math.min(devicePixelRatio,1.75));this.renderer.shadowMap.enabled=true;this.renderer.shadowMap.type=THREE.PCFSoftShadowMap;
    this.renderer.toneMapping=THREE.ACESFilmicToneMapping;this.renderer.toneMappingExposure=1.15;
    container.append(this.renderer.domElement);
    this.scene=new THREE.Scene();this.scene.background=new THREE.Color(0xdce4df);this.scene.fog=new THREE.Fog(0xdce4df,16,30);
    this.camera=new THREE.PerspectiveCamera(39,1,0.08,80);
    this.controls=new OrbitControls(this.camera,this.renderer.domElement);this.controls.target.set(0,1.05,0);this.controls.enableDamping=true;this.controls.dampingFactor=.075;
    this.controls.minDistance=5;this.controls.maxDistance=13;this.controls.maxPolarAngle=Math.PI*.45;this.controls.minPolarAngle=.2;this.controls.enablePan=false;
    this.controls.mouseButtons.RIGHT=THREE.MOUSE.ROTATE;this.controls.touches.TWO=THREE.TOUCH.DOLLY_ROTATE;
    this.resetCamera();
    this.room=new THREE.Group();this.scene.add(this.room);this.buildRoom();
    this.objects=new THREE.Group();this.scene.add(this.objects);this.leads=new THREE.Group();this.scene.add(this.leads);
    this.selection=new THREE.Mesh(new THREE.TorusGeometry(.56,.014,8,64),new THREE.MeshBasicMaterial({color:0x74ad54}));this.selection.rotation.x=Math.PI/2;this.selection.visible=false;this.scene.add(this.selection);
    this.ray=new THREE.Raycaster();this.pointer=new THREE.Vector2();this.plane=new THREE.Plane(new THREE.Vector3(0,1,0),-BENCH);this.drag=null;
    this.resizeObserver=new ResizeObserver(()=>this.resize());this.resizeObserver.observe(container);
    this.bindPointer();this.resize();
    const pmrem=new THREE.PMREMGenerator(this.renderer);pmrem.compileEquirectangularShader();
    new HDRLoader().load('/assets/studio_small_09_1k.hdr',texture=>{
      const target=pmrem.fromEquirectangular(texture);this.scene.environment=target.texture;this.environment=target;texture.dispose();pmrem.dispose();
    },undefined,()=>{pmrem.dispose();this.callbacks.notice('Environment lighting unavailable; using local scene lights.');});
    let last=0;const render=now=>{this.frame=requestAnimationFrame(render);if(now-last<1000/60-1)return;last=now;this.controls.update();this.renderer.render(this.scene,this.camera);};this.frame=requestAnimationFrame(render);
    this.renderer.domElement.addEventListener('webglcontextlost',event=>{event.preventDefault();this.callbacks.notice('Graphics context lost. Your saved draft is safe; reload to restore the bench.');});
    document.addEventListener('visibilitychange',()=>{if(document.hidden)this.controls.autoRotate=false;});
  }
  resize(){const {width,height}=this.container.getBoundingClientRect();if(!width||!height)return;this.renderer.setSize(width,height,false);this.camera.aspect=width/height;this.camera.fov=Math.min(68,39/Math.min(1,width/height/1.2));this.camera.updateProjectionMatrix();}
  resetCamera(){this.camera.position.set(4.5,6.4,8.8);this.controls.target.set(0,1.05,0);this.controls.update();}
  quality(low){this.renderer.setPixelRatio(Math.min(devicePixelRatio,low?1:1.75));this.renderer.shadowMap.enabled=!low;this.resize();}
  material(color,options={}){const mat=new THREE.MeshStandardMaterial({color,roughness:.4,metalness:0,...options});this.dynamicMaterials.add(mat);return mat;}
  mesh(parent,geometry,material,x=0,y=0,z=0){const m=new THREE.Mesh(geometry,material);m.position.set(x,y,z);m.castShadow=true;m.receiveShadow=true;parent.add(m);return m;}
  box(parent,w,h,d,x,y,z,material){return this.mesh(parent,new THREE.BoxGeometry(w,h,d),material,x,y,z);}
  cylinder(parent,r1,r2,h,x,y,z,material,segments=40){return this.mesh(parent,new THREE.CylinderGeometry(r1,r2,h,segments),material,x,y,z);}
  sphere(parent,r,x,y,z,material,sx=1,sy=1,sz=1){const m=this.mesh(parent,new THREE.SphereGeometry(r,32,20),material,x,y,z);m.scale.set(sx,sy,sz);return m;}
  line(parent,points,r,material){const curve=new THREE.CatmullRomCurve3(points.map(p=>new THREE.Vector3(...p)));return this.mesh(parent,new THREE.TubeGeometry(curve,Math.max(12,points.length*6),r,8,false),material);}
  torus(parent,r,tube,x,y,z,material){const ring=this.mesh(parent,new THREE.TorusGeometry(r,tube,8,48),material,x,y,z);ring.rotation.x=Math.PI/2;return ring;}
  label(parent,text,x,y,z,width=.8,height=.18,bg='#e8eee2',fg='#274c40') {
    const canvas=document.createElement('canvas');canvas.width=512;canvas.height=Math.round(512*height/width);const ctx=canvas.getContext('2d');
    if(bg){ctx.fillStyle=bg;ctx.fillRect(0,0,canvas.width,canvas.height);}ctx.fillStyle=fg;ctx.font=`500 ${Math.round(canvas.height*.53)}px 'Segoe UI',Tajawal,sans-serif`;ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillText(text,256,canvas.height/2,490);
    const texture=new THREE.CanvasTexture(canvas);texture.colorSpace=THREE.SRGBColorSpace;
    const mat=new THREE.SpriteMaterial({map:texture,transparent:true,depthWrite:false});this.dynamicMaterials.add(mat);
    const sprite=new THREE.Sprite(mat);sprite.position.set(x,y,z);sprite.scale.set(width,height,1);parent.add(sprite);return sprite;
  }
  flatLabel(parent,text,x,y,z,width=.6,height=.2,bg='#182d28',fg='#abdcb7'){
    const sprite=this.label(parent,text,x,y,z,width,height,bg,fg);parent.remove(sprite);
    const plane=this.mesh(parent,new THREE.PlaneGeometry(width,height),new THREE.MeshBasicMaterial({map:sprite.material.map,transparent:true}),x,y,z);
    this.dynamicMaterials.add(plane.material);plane.rotation.x=-Math.PI/2;return plane;
  }
  buildRoom(){
    const room=this.room,ivory=this.material(0xe4e7db,{roughness:.65}),metal=this.material(0x9faeaa,{metalness:.85,roughness:.27}),dark=this.material(0x2c3b38,{roughness:.25,metalness:.25}),wall=this.material(0xe7ece2,{roughness:.85});
    this.scene.add(new THREE.HemisphereLight(0xeaf7ec,0x72817a,2));
    const sun=new THREE.DirectionalLight(0xfff4dc,3.4);sun.position.set(-4,8,4);sun.castShadow=true;sun.shadow.mapSize.set(2048,2048);Object.assign(sun.shadow.camera,{left:-8,right:8,top:7,bottom:-7,near:.1,far:25});sun.shadow.normalBias=.025;sun.shadow.bias=-.0002;this.scene.add(sun);
    const fill=new THREE.DirectionalLight(0xc5e7fa,1.5);fill.position.set(5,5,-3);this.scene.add(fill);
    this.box(room,30,.15,30,0,-.1,0,this.material(0xadb8b0,{roughness:.8}));
    const grid=new THREE.GridHelper(30,30,0x99aaa0,0xb8c3ba);grid.position.y=-.02;room.add(grid);
    this.box(room,17,7,.16,0,3.4,-4.5,wall);this.box(room,.16,7,17,-8.5,3.4,0,wall);
    this.box(room,17,.4,.02,0,1.5,-4.39,this.material(0x3a7973));
    this.box(room,7.5,.15,3.9,0,BENCH-.075,0,dark);
    this.box(room,7.25,.65,3.65,0,.36,0,ivory);
    this.box(room,7.1,.13,3.5,0,.06,0,this.material(0x4e5f58));
    for(let x=-2.7;x<=2.8;x+=1.35){
      this.box(room,1.30,.49,.04,x,.41,1.84,ivory);
      this.box(room,1.27,.015,.015,x,.40,1.87,metal);
      this.box(room,.37,.025,.035,x,.56,1.88,metal);
    }
    this.box(room,8,.12,1.25,0,1.08,-3.45,dark);
    for(let x=-3.3;x<=3.4;x+=1.35){this.box(room,1.3,1.02,.85,x,.52,-3.53,ivory);this.box(room,.4,.025,.04,x,.85,-3.08,metal);}
    for(let x=-1;x<2;x+=.5){
      const bottle=new THREE.Group();bottle.position.set(x,1.14,-3.5);room.add(bottle);
      this.cylinder(bottle,.105,.11,.27,0,.135,0,this.material(0x785132,{roughness:.19}));this.cylinder(bottle,.065,.065,.08,0,.31,0,dark);this.box(bottle,.12,.13,.01,0,.14,.105,ivory);
    }
    for(let x=1.7;x<5;x+=1.05){this.box(room,1,1.5,.43,x,3.15,-4.1,ivory);this.box(room,.025,.28,.02,x+.33,3.07,-3.87,metal);}
    const windowMat=this.material(0xcde8e8,{emissive:0xc2e4e2,emissiveIntensity:.35,roughness:.15});
    this.box(room,3.2,2.55,.04,-3.4,3.35,-4.36,windowMat);
    for(const x of [-5.05,-3.4,-1.75])this.box(room,.075,2.7,.12,x,3.35,-4.3,ivory);
    for(const y of [2,3.35,4.7])this.box(room,3.4,.075,.12,-3.4,y,-4.3,ivory);
    this.label(room,'V I R T U A L   S T E M   L A B',.4,2.05,-4.25,3.6,.22,null,'#71928a');
    this.roomMaterialCount=this.dynamicMaterials.size;
  }
  disposeGroup(group){group.traverse(node=>{if(node.geometry)node.geometry.dispose();if(node.material){const mats=Array.isArray(node.material)?node.material:[node.material];for(const mat of mats){if(mat.map)mat.map.dispose();mat.dispose();this.dynamicMaterials.delete(mat);}}});group.clear();}
  update(state,readings,definitions,elements,language='en'){
    this.state=state;this.readings=readings;this.definitions=definitions;this.elements=elements;this.language=language;
    // Each equipment group owns its geometry and materials; rebuilding releases their GPU resources.
    this.disposeGroup(this.objects);this.disposeGroup(this.leads);this.groups.clear();this.ports.clear();this.pickables=[];
    for(const tool of state.Tools){
      const group=new THREE.Group();group.position.set(tool.X,BENCH,tool.Z);group.rotation.y=tool.Yaw;group.userData.tool=tool.Id;this.objects.add(group);this.groups.set(tool.Id,group);
      this.buildTool(group,tool,readings[tool.Id]||{},state.Experiment);
      group.traverse(node=>{if(node.isMesh){node.userData.tool=tool.Id;this.pickables.push(node);}});
      if(this.electrical(tool.Kind,state.Experiment))this.addPorts(group,tool);
    }
    this.drawWires();this.select(state.SelectedId);
  }
  electrical(kind,experiment){return experiment==='electricity'&&(kind==='meter'||['sources','passive','semiconductors','electrical-meters'].includes(this.definitions.find(d=>d.Kind===kind)?.Category));}
  addPorts(group,tool){
    for(const [index,suffix] of ['a','b'].entries()){
      const x=index===0?-.58:.58,z=.38;
      this.cylinder(group,.095,.095,.05,x,.04,z,this.material(colors.black));
      this.torus(group,.067,.019,x,.075,z,this.material(index===0?colors.red:0x58615c,{metalness:.5}));
      const hit=this.sphere(group,.063,x,.092,z,this.material(index===0?0xd55a4d:0x323c39,{metalness:.3}));hit.userData.terminal=tool.Id+'.'+suffix;hit.userData.tool=tool.Id;this.pickables.push(hit);
      this.ports.set(tool.Id+'.'+suffix,{group,position:new THREE.Vector3(x,.09,z)});
    }
  }
  portPosition(id){const port=this.ports.get(id);return port?port.group.localToWorld(port.position.clone()):null;}
  drawWires(){
    this.disposeGroup(this.leads);
    for(const [i,wire] of (this.state?.Wires||[]).entries()){
      const a=this.portPosition(wire.A),b=this.portPosition(wire.B);if(!a||!b)continue;
      const mid=a.clone().lerp(b,.5);mid.y=BENCH+.1;mid.z+=.4+(i%3)*.2;
      const near=a.clone().lerp(b,.12);near.y+=.12;const far=a.clone().lerp(b,.88);far.y+=.12;
      this.line(this.leads,[a.toArray(),near.toArray(),mid.toArray(),far.toArray(),b.toArray()],.019,this.material(i%2===0?0xc45248:0x284b45,{roughness:.5}));
    }
  }
  select(id){this.selected=id;const group=this.groups.get(id);this.selection.visible=!!group;if(group)this.selection.position.set(group.position.x,BENCH+.016,group.position.z);}
  buildTool(group,tool,reading,experiment){
    const kind=tool.Kind,def=this.definitions.find(d=>d.Kind===kind);
    const mat=(color,options)=>this.material(color,options);
    if(def?.Behavior==='fluid'){this.vessel(group,tool);return;}
    if(kind==='element'){
      const element=this.elements.find(e=>e.Symbol===tool.Mode);this.cylinder(group,.24,.24,.055,0,.03,0,mat(colors.dark));
      this.cylinder(group,.2,.2,.5,0,.3,0,mat(0xc5e1d9,{transparent:true,opacity:.28,roughness:.1,metalness:.1}));
      this.cylinder(group,.225,.225,.07,0,.59,0,mat(colors.teal,{metalness:.4}));
      this.sphere(group,.11,0,.18,0,mat('#'+(element?.Color||'ab8b5a'),{metalness:.7,roughness:.25}),1,.8,1);
      this.label(group,tool.Mode,0,.43,.22,.31,.19,'#f3f2dd');this.label(group,`${this.num(reading.mass)} g`,0,.79,0,.74,.16);return;
    }
    if(kind==='battery'){
      this.box(group,.94,.44,.62,0,.25,0,mat(0xe0e0d2));this.box(group,.86,.34,.025,0,.25,.325,mat(0x354440));
      this.label(group,`${tool.Value.toFixed(2)} V`,-.10,.29,.35,.5,.15,'#183c35','#7ee3c1');
      this.cylinder(group,.07,.07,.04,.31,.27,.35,mat(colors.black)).rotation.x=Math.PI/2;
      for(let x=-.31;x<=.32;x+=.08)this.box(group,.025,.012,.28,x,.481,-.04,mat(colors.black));
      this.label(group,'DC POWER',0,.57,0,.7,.12);return;
    }
    if(kind==='meter'||kind==='voltmeter'||kind==='ammeter'||kind==='phmeter'||kind==='thermometer'){
      const chem=experiment==='chemistry',color=kind==='meter'&&!chem?0xdfb842:chem?0xcedac7:0xa0c2aa;
      this.box(group,.57,.15,.87,0,.1,0,mat(color,{roughness:.65}));this.box(group,.47,.02,.76,0,.19,0,mat(colors.dark));
      const measure=reading.instrument,txt=measure?.value!=null?this.num(measure.value)+' '+measure.unit:'—';
      this.flatLabel(group,txt,0,.211,-.21,.41,.20,'#a9c5a5','#203e2f');
      this.cylinder(group,.125,.125,.045,0,.227,.11,mat(colors.black));this.box(group,.025,.012,.15,0,.257,.11,mat(0xbbc9bd));
      if(chem)this.line(group,[[.2,.12,.4],[.5,.16,.5],[.7,.08,.2],[.75,.15,-.1]],.015,mat(colors.black));
      this.label(group,kind==='meter'?(chem?'mol / L':'MULTIMETER'):kind.toUpperCase(),0,.48,0,.85,.13);return;
    }
    if(kind==='balance'){
      this.box(group,.89,.13,.55,0,.08,0,mat(0xb4bcab));this.cylinder(group,.15,.19,.33,0,.3,0,mat(0x58776b,{metalness:.5}));
      for(let z=-.12;z<=.12;z+=.12){this.box(group,.9,.035,.035,0,.51,z,mat(colors.metal,{metalness:.8}));this.box(group,.075,.09,.065,.15+z,.54,z,mat(colors.dark));}
      this.cylinder(group,.26,.28,.04,-.35,.62,0,mat(colors.metal,{metalness:.9}));this.label(group,reading.instrument?.value!=null?this.num(reading.instrument.value)+' g':'BALANCE',0,.84,0,.9,.15);return;
    }
    if(def?.Behavior==='logic'){
      this.box(group,.88,.08,.64,0,.055,0,mat(0x2c6952));this.box(group,.29,.095,.30,0,.14,0,mat(colors.black));
      for(let x=-.22;x<=.22;x+=.11){this.box(group,.026,.025,.12,x,.105,.18,mat(colors.metal,{metalness:.8}));this.box(group,.026,.025,.12,x,.105,-.18,mat(colors.metal,{metalness:.8}));}
      for(let i=0;i<2;i++){this.box(group,.11,.04,.13,-.31,.13,-.15+i*.3,mat(tool.Mode[i]==='1'?0xb7d873:colors.ivory));}
      this.sphere(group,.07,.31,.16,0,mat(reading.output?0x9add53:0x3b5231,{emissive:reading.output?0x74b32a:0,emissiveIntensity:.8}));this.label(group,`${kind.toUpperCase()}   Y=${reading.output?1:0}`,0,.42,0,1,.16);return;
    }
    if(def?.Behavior==='reference'){this.apparatus(group,tool);return;}
    this.box(group,.85,.075,.56,0,.045,0,mat(colors.ivory));
    if(kind==='resistor'||kind==='rheostat'){
      const r=this.cylinder(group,.12,.12,.55,0,.22,0,mat(kind==='rheostat'?0xc4c2b4:0xd5c393));r.rotation.z=Math.PI/2;
      for(let x=-.2;x<=.21;x+=.1){const band=this.cylinder(group,.122,.122,.033,x,.22,0,mat(kind==='rheostat'?colors.metal:[0x674b30,0x343b37,0xa47843][Math.round((x+.2)*10)%3],{metalness:kind==='rheostat'?.8:0}));band.rotation.z=Math.PI/2;}
      this.line(group,[[-.42,.08,.15],[-.36,.22,0],[-.27,.22,0]],.015,mat(colors.metal,{metalness:.8}));this.line(group,[[.27,.22,0],[.37,.22,0],[.43,.08,.15]],.015,mat(colors.metal,{metalness:.8}));
      if(kind==='rheostat'){this.box(group,.05,.28,.04,.05,.31,0,mat(colors.metal));this.box(group,.22,.04,.12,.05,.46,0,mat(colors.black));}
      this.label(group,`${this.num(tool.Value)} Ω`,0,.58,0,.8,.14);
    }else if(kind==='switch'){
      this.cylinder(group,.05,.06,.1,-.27,.15,0,mat(colors.copper,{metalness:.8}));this.cylinder(group,.05,.06,.1,.27,.15,0,mat(colors.copper,{metalness:.8}));
      const end=tool.Mode==='closed'?[.27,.22,0]:[.18,.56,0];this.line(group,[[-.27,.22,0],end],.035,mat(colors.metal,{metalness:.8}));this.sphere(group,.065,...end,mat(colors.dark));this.label(group,tool.Mode.toUpperCase(),0,.73,0,.7,.13);
    }else if(kind==='bulb'){
      this.cylinder(group,.22,.26,.09,0,.12,0,mat(colors.ivory));this.cylinder(group,.13,.13,.2,0,.26,0,mat(colors.metal,{metalness:.9}));
      this.sphere(group,.28,0,.57,0,mat(0xdfebe0,{transparent:true,opacity:.27,roughness:.07,metalness:.12}),1,1.2,1);
      const glow=Math.min(1,Math.max(0,reading.branch?.PowerW||0)/.2);this.line(group,[[-.04,.35,0],[-.07,.56,0],[.07,.56,0],[.04,.35,0]],.01,mat(glow?0xf6b648:0x5f6456,{emissive:0xffa323,emissiveIntensity:glow*2}));
      this.label(group,`${this.num(reading.branch?.PowerW)} W`,0,1.02,0,.8,.14);
    }else if(kind==='capacitor'){
      this.cylinder(group,.2,.2,.56,0,.37,0,mat(0x397a85,{metalness:.6,roughness:.25}));this.cylinder(group,.18,.18,.025,0,.662,0,mat(colors.metal,{metalness:.9}));
      this.label(group,`${this.num(tool.Value*1e6)} μF`,0,.86,0,.9,.14);
    }else if(kind==='inductor'){
      const pts=[];for(let i=0;i<=500;i++){const t=i/500;pts.push([-.31+t*.62,.3+Math.cos(t*Math.PI*32)*.17,Math.sin(t*Math.PI*32)*.17]);}
      this.line(group,pts,.018,mat(colors.copper,{metalness:.85,roughness:.24}));this.label(group,`${this.num(tool.Value)} H`,0,.67,0,.8,.14);
    }else if(kind==='led'||kind==='diode'){
      if(kind==='led'){
        const on=(reading.branch?.CurrentA||0)>.0005;
        this.cylinder(group,.13,.13,.23,0,.25,0,mat(on?0xe54f3c:0x8d302b,{transparent:true,opacity:.87,emissive:on?0xe93010:0,emissiveIntensity:on?.75:0}));
        this.sphere(group,.13,0,.365,0,mat(on?0xed5b41:0x8d302b,{emissive:on?0xd52a13:0,emissiveIntensity:on?.7:0}),1,1,1);
      }else{const body=this.cylinder(group,.085,.085,.36,0,.25,0,mat(colors.black));body.rotation.z=Math.PI/2;const band=this.cylinder(group,.087,.087,.045,.12,.25,0,mat(colors.metal));band.rotation.z=Math.PI/2;}
      this.line(group,[[-.42,.08,.16],[-.26,.16,0],[-.1,.24,0]],.015,mat(colors.metal));this.line(group,[[.1,.24,0],[.27,.16,0],[.42,.08,.16]],.015,mat(colors.metal));this.label(group,`${kind.toUpperCase()}  ${tool.Value} V`,0,.7,0,1,.14);
    }
  }
  num(value){if(value==null)return '—';if(value===0)return '0';return Math.abs(value)<.001||Math.abs(value)>=10000?value.toExponential(2):Number(value.toPrecision(4)).toString();}
  vessel(group,tool){
    const kind=tool.Kind,mat=(c,o)=>this.material(c,o),metal=mat(colors.metal,{metalness:.8,roughness:.25});
    let profile=[[.34,.06],[.36,.1],[.36,.92]],height=.92;
    if(kind==='cylinder')profile=[[.18,.08],[.18,1.35]];
    if(kind==='erlenmeyer')profile=[[.38,.06],[.42,.12],[.18,.90],[.17,1.2]];
    if(kind==='florence')profile=[[.18,.06],[.32,.13],[.43,.38],[.36,.70],[.14,.92],[.14,1.25]];
    if(kind==='testtube')profile=[[.025,.04],[.11,.09],[.13,.19],[.13,1.08]];
    if(kind==='burette')profile=[[.035,.24],[.07,.38],[.07,1.68]];
    if(kind==='pipette')profile=[[.012,.05],[.045,.22],[.045,1.22]];
    if(kind==='volpipette')profile=[[.012,.05],[.038,.26],[.038,.43],[.12,.54],[.12,.7],[.038,.83],[.038,1.26]];
    if(kind==='funnel')profile=[[.035,.06],[.035,.42],[.38,.88]];
    if(kind==='watchglass')profile=[[.1,.035],[.3,.06],[.43,.12]];
    if(kind==='stock'||kind==='water')profile=[[.29,.06],[.31,.12],[.31,.73],[.12,.91],[.12,1.05]];
    height=profile.at(-1)[1];
    const glass=new THREE.MeshPhysicalMaterial({color:0xe3f5f1,roughness:.08,metalness:0,transmission:.94,thickness:.025,ior:1.46,transparent:true,opacity:.76,side:THREE.DoubleSide,depthWrite:false});this.dynamicMaterials.add(glass);
    const outer=profile.map(([r,y])=>new THREE.Vector2(r,y));const inner=[...profile].reverse().map(([r,y])=>new THREE.Vector2(Math.max(.005,r-.018),y+.003));
    const points=[new THREE.Vector2(0,profile[0][1]),...outer,...inner,new THREE.Vector2(0,profile[0][1]+.025)];
    this.mesh(group,new THREE.LatheGeometry(points,64),glass);
    this.torus(group,profile.at(-1)[0],.014,0,height,0,mat(0xbdceca,{metalness:.3,transparent:true,opacity:.65,roughness:.16}));
    if(kind==='cylinder')this.cylinder(group,.32,.32,.055,0,.03,0,glass.clone());
    if(kind==='burette'){
      this.box(group,.5,.06,.48,.24,.025,0,metal);this.cylinder(group,.018,.018,1.6,.24,.83,0,metal);
      this.line(group,[[.24,1.1,0],[.1,1.1,0],[.08,1.1,0]],.025,metal);
      this.box(group,.22,.035,.055,0,.28,0,mat(0xb9634a));
    }
    if(kind==='stock'||kind==='water'){
      this.cylinder(group,.145,.145,.12,0,1.1,0,mat(kind==='water'?0x477c84:0x323f34,{roughness:.7}));
      this.label(group,kind==='water'?'H₂O':(tool.SoluteId==='ideal-solute'?'STOCK':tool.SoluteId||'STOCK'),0,.50,.32,.5,.2,'#e8e9d1');
    }
    if(tool.VolumeL>0){
      // Integrate r² dy to find fill height, preserving the geometry's volume fraction in tapered vessels.
      const segments=[];let total=0;
      for(let i=0;i<profile.length-1;i++){
        const [r0,y0]=profile[i],[r1,y1]=profile[i+1];
        const volume=(y1-y0)*(r0*r0+r0*r1+r1*r1)/3;segments.push({r0,y0,r1,y1,volume});total+=volume;
      }
      const wanted=total*Math.min(1,tool.VolumeL/tool.CapacityL);let sum=0,fillY=profile[0][1],fillR=profile[0][0];const fillPoints=[new THREE.Vector2(0,profile[0][1]+.026),new THREE.Vector2(Math.max(.005,profile[0][0]-.024),profile[0][1]+.026)];
      for(const s of segments){
        if(sum+s.volume<=wanted){fillY=s.y1;fillR=s.r1;fillPoints.push(new THREE.Vector2(Math.max(.005,s.r1-.024),s.y1));sum+=s.volume;continue;}
        let lo=0,hi=1;for(let i=0;i<32;i++){const t=(lo+hi)/2,r=s.r0+(s.r1-s.r0)*t,v=(s.y1-s.y0)*t*(s.r0*s.r0+s.r0*r+r*r)/3;if(sum+v<wanted)lo=t;else hi=t;}
        const t=(lo+hi)/2;fillY=s.y0+(s.y1-s.y0)*t;fillR=s.r0+(s.r1-s.r0)*t;fillPoints.push(new THREE.Vector2(Math.max(.005,fillR-.024),fillY));break;
      }
      fillPoints.push(new THREE.Vector2(0,fillY));
      const hue=tool.SoluteId==='CuSO4'?0x288bb0:tool.SoluteId==='FeCl3'?0xa7732e:tool.SoluteId==='ideal-solute'?0x4b9db0:0x87bdb5;
      this.mesh(group,new THREE.LatheGeometry(fillPoints,64),mat(hue,{transparent:true,opacity:.63,roughness:.13,metalness:.07,side:THREE.DoubleSide}));
      if(fillR>.04)this.torus(group,Math.max(.01,fillR-.025),.006,0,fillY,0,mat(0xc4e7e4,{transparent:true,opacity:.7}));
    }
    if(!['stock','water','watchglass','funnel'].includes(kind)){
      for(let i=1;i<=4;i++){
        const y=profile[0][1]+(height-profile[0][1])*i/5;let radius=profile[0][0];
        for(let p=0;p<profile.length-1;p++)if(y>=profile[p][1]&&y<=profile[p+1][1]){const a=profile[p],b=profile[p+1];radius=a[0]+(b[0]-a[0])*(y-a[1])/(b[1]-a[1]);}
        this.box(group,radius*.55,.008,.004,0,y,radius+.004,mat(0x788d85,{transparent:true,opacity:.7}));
      }
    }
    this.label(group,`${Math.round(tool.VolumeL*1000*10)/10} / ${Math.round(tool.CapacityL*1000)} mL`,0,height+.2,0,.9,.14);
  }
  apparatus(group,tool){
    const kind=tool.Kind,mat=(c,o)=>this.material(c,o),metal=mat(colors.metal,{metalness:.85,roughness:.24}),ceramic=mat(colors.ivory,{roughness:.48}),dark=mat(colors.dark),glass=mat(0xa8c6c0,{transparent:true,opacity:.4,roughness:.12});
    if(['stand','clamp','holder'].includes(kind)){
      this.box(group,.64,.07,.5,0,.04,0,dark);this.cylinder(group,.025,.025,1.25,.18,.7,0,metal);
      this.line(group,[[.18,.95,0],[-.18,.95,0]],.025,metal);this.torus(group,.18,.018,-.17,.95,0,metal);
      if(kind!=='stand')this.box(group,.25,.045,.13,-.18,.96,0,dark);
    }else if(kind==='condenser'){
      const body=this.cylinder(group,.13,.13,.95,0,.6,0,glass);body.rotation.z=-.3;
      this.line(group,[[.1,.12,0],[-.1,1.1,0]],.035,glass);for(let i=0;i<6;i++)this.torus(group,.09,.012,-.01*i,.28+i*.12,0,metal);
      this.line(group,[[.1,.4,0],[.32,.43,0]],.025,glass);this.line(group,[[-.1,.9,0],[-.3,.9,0]],.025,glass);
    }else if(kind==='mortar'){
      const points=[[0,.05],[.24,.05],[.35,.35],[.31,.4],[.23,.15],[0,.13]].map(p=>new THREE.Vector2(...p));this.mesh(group,new THREE.LatheGeometry(points,48),ceramic);
      this.line(group,[[.02,.15,0],[.27,.6,0]],.06,ceramic);
    }else if(kind==='crucible'){
      this.cylinder(group,.21,.15,.34,0,.2,0,ceramic);this.cylinder(group,.25,.22,.045,0,.39,0,ceramic);this.sphere(group,.04,0,.44,0,ceramic);
    }else if(kind==='burner'){
      this.cylinder(group,.28,.3,.06,0,.04,0,dark);this.cylinder(group,.065,.08,.67,0,.4,0,metal);this.torus(group,.087,.017,0,.22,0,dark);this.box(group,.2,.07,.08,.12,.12,0,metal);
    }else if(kind==='bulb-pipette'){
      this.sphere(group,.21,0,.27,0,mat(0xa3493f),1,1.3,1);this.cylinder(group,.05,.065,.2,0,.55,0,mat(0xa3493f));
    }else if(kind==='brush'){
      this.line(group,[[-.3,.07,0],[.25,.11,0]],.02,metal);const brush=this.cylinder(group,.075,.075,.3,.27,.11,0,ceramic);brush.rotation.z=Math.PI/2;
      for(let i=0;i<7;i++){const ring=this.torus(group,.08,.009,.15+i*.04,.11,0,mat(0xc0bda8));ring.rotation.z=Math.PI/2;}
    }else if(kind==='scoopula'){
      this.box(group,.72,.025,.08,0,.08,0,metal);this.sphere(group,.12,.3,.09,0,metal,1.5,.25,.8);
    }else if(kind==='stopper'){
      this.cylinder(group,.17,.13,.25,0,.16,0,dark);this.cylinder(group,.025,.025,.26,0,.17,0,metal);
    }else if(['ceramic','gauze','triangle'].includes(kind)){
      if(kind==='triangle')this.line(group,[[-.3,.05,.2],[.3,.05,.2],[0,.05,-.32],[-.3,.05,.2]],.035,ceramic);
      else{this.box(group,.65,.035,.65,0,.03,0,kind==='ceramic'?ceramic:metal);if(kind==='gauze'){for(let i=-.25;i<=.25;i+=.05){this.box(group,.008,.006,.6,i,.055,0,dark);this.box(group,.6,.006,.008,0,.055,i,dark);}}}
    }else if(kind==='tongs'){
      this.line(group,[[-.32,.08,-.3],[.05,.09,.25],[.21,.09,.36]],.018,metal);this.line(group,[[.32,.08,-.3],[-.05,.09,.25],[-.21,.09,.36]],.018,metal);this.cylinder(group,.035,.035,.03,0,.11,.15,metal);
    }else if(kind==='transistor'){
      this.box(group,.33,.4,.16,0,.47,0,dark);for(let i=-1;i<=1;i++)this.line(group,[[i*.11,.27,0],[i*.16,.07,.15]],.012,metal);this.label(group,'NPN',0,.48,.1,.3,.12);
    }else if(kind==='relay'){
      this.box(group,.6,.07,.55,0,.05,0,dark);this.box(group,.45,.44,.38,0,.31,0,glass);this.cylinder(group,.13,.13,.26,0,.3,0,mat(colors.copper,{metalness:.7}));
    }else if(kind==='generator'){
      this.box(group,.85,.4,.6,0,.23,0,ceramic);this.label(group,'∿ SIGNAL',0,.3,.32,.65,.16,'#254238','#a5d8b8');
    }
    const name=this.definitions.find(d=>d.Kind===kind)?.[this.language==='ar'?'Arabic':'English']||kind;
    this.label(group,name,0,kind==='stand'||kind==='clamp'||kind==='holder'?1.55:.85,0,1.1,.15);
  }
  bindPointer(){
    const canvas=this.renderer.domElement;
    const intersection=event=>{const rect=canvas.getBoundingClientRect();this.pointer.set((event.clientX-rect.left)/rect.width*2-1,-(event.clientY-rect.top)/rect.height*2+1);this.ray.setFromCamera(this.pointer,this.camera);return this.ray.intersectObjects(this.pickables,false)[0];};
    canvas.addEventListener('pointerdown',event=>{
      if(event.button!==0)return;const hit=intersection(event);if(!hit)return;
      if(hit.object.userData.terminal){this.controls.enabled=false;this.terminalDown={id:hit.object.userData.terminal,x:event.clientX,y:event.clientY};return;}
      const id=hit.object.userData.tool,group=this.groups.get(id);if(!group)return;
      this.callbacks.select(id);this.select(id);const point=new THREE.Vector3();this.ray.ray.intersectPlane(this.plane,point);
      this.drag={id,group,startX:event.clientX,startY:event.clientY,offset:group.position.clone().sub(point),original:group.position.clone(),moved:false};this.controls.enabled=false;canvas.setPointerCapture(event.pointerId);
    },true);
    canvas.addEventListener('pointermove',event=>{
      if(!this.drag){const hit=intersection(event);canvas.style.cursor=hit?(hit.object.userData.terminal?'crosshair':'grab'):'default';return;}
      intersection(event);const point=new THREE.Vector3();if(!this.ray.ray.intersectPlane(this.plane,point))return;
      if(Math.hypot(event.clientX-this.drag.startX,event.clientY-this.drag.startY)>4)this.drag.moved=true;
      if(this.drag.moved){point.add(this.drag.offset);this.drag.group.position.set(THREE.MathUtils.clamp(point.x,-3,3),BENCH,THREE.MathUtils.clamp(point.z,-1.5,1.5));this.select(this.drag.id);this.drawWires();canvas.style.cursor='grabbing';}
    });
    const end=event=>{
      this.controls.enabled=true;
      if(this.terminalDown){if(event.type==='pointerup'&&Math.hypot(event.clientX-this.terminalDown.x,event.clientY-this.terminalDown.y)<8)this.callbacks.terminal(this.terminalDown.id);this.terminalDown=null;}
      const drag=this.drag;this.drag=null;if(!drag)return;
      if(event.type==='pointercancel'){drag.group.position.copy(drag.original);this.drawWires();this.select(drag.id);return;}
      if(drag.moved)this.callbacks.move(drag.id,Math.round(drag.group.position.x*20)/20,Math.round(drag.group.position.z*20)/20);
      canvas.style.cursor='grab';
    };
    canvas.addEventListener('pointerup',end);canvas.addEventListener('pointercancel',end);canvas.addEventListener('lostpointercapture',()=>{this.controls.enabled=true;});
    canvas.addEventListener('contextmenu',event=>event.preventDefault());
  }
  projected(id,terminal){const p=terminal?this.portPosition(id+'.'+terminal):this.groups.get(id)?.position.clone().add(new THREE.Vector3(0,.3,0));if(!p)return null;p.project(this.camera);const rect=this.renderer.domElement.getBoundingClientRect();return {x:rect.left+(p.x+1)*rect.width/2,y:rect.top+(1-p.y)*rect.height/2};}
}
