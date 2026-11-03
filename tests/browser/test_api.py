"""End-to-end checks against the running browser host; no browser or third-party Python packages required."""
import copy
import json
import os
import unittest
import urllib.error
import urllib.request

BASE=os.environ.get('STEM_BROWSER_URL','http://127.0.0.1:5080')


def http(path, data=None, headers=None):
    request=urllib.request.Request(BASE+path, data=None if data is None else json.dumps(data).encode(),
                                   headers={'Content-Type':'application/json',**(headers or {})})
    try:
        with urllib.request.urlopen(request,timeout=15) as response:
            raw=response.read();return response.status,json.loads(raw) if 'json' in response.headers.get('Content-Type','') else raw
    except urllib.error.HTTPError as error:
        raw=error.read()
        try:return error.code,json.loads(raw)
        except ValueError:return error.code,raw


class BrowserApi(unittest.TestCase):
    def setUp(self):
        code,result=http('/api/workspace');self.assertEqual(code,200);self.workspace=result['workspace'];self.result=result

    def command(self,action,**fields):
        code,result=http('/api/command',{'Action':action,'Workspace':self.workspace,**fields})
        self.assertEqual(code,200,result);self.workspace=result['workspace'];self.result=result;return result

    def circuit(self,preset):
        self.workspace['ActiveExperiment']='electricity';return self.command('preset',Content=preset)

    def empty(self,experiment='chemistry'):
        self.workspace['ActiveExperiment']=experiment
        key='Chemistry' if experiment=='chemistry' else 'Electricity'
        self.workspace[key]['Tools']=[];self.workspace[key]['Wires']=[];self.workspace[key]['SelectedId']=''
        self.workspace[key+'Undo']=[]

    def test_catalog_offline_assets_and_local_boundary(self):
        code,catalog=http('/api/catalog');self.assertEqual(code,200)
        self.assertEqual(len(catalog['tools']),52);self.assertEqual(len(catalog['elements']),118);self.assertEqual(len(catalog['reagents']),11)
        self.assertEqual([e['Number'] for e in catalog['elements']],list(range(1,119)))
        for path in ['/','/app.js','/scene.js','/style.css','/vendor/three.module.js','/vendor/three.core.js','/vendor/OrbitControls.js','/vendor/HDRLoader.js','/assets/studio_small_09_1k.hdr']:
            self.assertEqual(http(path)[0],200,path)
        self.assertEqual(http('/api/health',headers={'Host':'untrusted.example'})[0],403)
        self.assertEqual(http('/api/command',{'Action':'view','Workspace':self.workspace},headers={'Origin':'https://untrusted.example'})[0],403)

    def test_dilution_transfer_conservation_undo_and_rejection(self):
        self.command('transfer',Tool='stock',Target='beaker',Value=25)
        self.assertAlmostEqual(self.result['readings']['beaker']['concentration'],2)
        self.command('transfer',Tool='water',Target='beaker',Value=75)
        self.assertAlmostEqual(self.result['readings']['beaker']['concentration'],.5)
        tools=self.workspace['Chemistry']['Tools'];self.assertAlmostEqual(sum(t['Moles'] for t in tools),.2)
        self.assertAlmostEqual(sum(t['VolumeL'] for t in tools),.6)
        before=copy.deepcopy(self.workspace)
        code,result=http('/api/command',{'Action':'transfer','Workspace':self.workspace,'Tool':'water','Target':'beaker','Value':500})
        self.assertEqual(code,400);self.assertEqual(before,self.workspace)
        self.command('undo');self.assertAlmostEqual(self.result['readings']['beaker']['concentration'],2)

    def test_ph_mass_and_probe_proximity(self):
        self.empty();self.command('add',Kind='stock',Content='HCl')
        acid=self.workspace['Chemistry']['Tools'][0];self.assertAlmostEqual(self.result['readings'][acid['Id']]['ph'],1,places=10)
        self.command('move',Tool=acid['Id'],X=0,Z=0)
        self.command('add',Kind='phmeter');probe=self.workspace['Chemistry']['Tools'][-1]
        self.command('target',Tool=probe['Id'],Target=acid['Id'])
        self.assertIsNone(self.result['readings'][probe['Id']]['instrument']['value'])
        self.command('move',Tool=probe['Id'],X=1.25,Z=0)
        self.assertAlmostEqual(self.result['readings'][probe['Id']]['instrument']['value'],1,places=10)
        self.empty();self.command('add',Kind='element',Content='Cu');sample=self.workspace['Chemistry']['Tools'][0]
        self.assertAlmostEqual(self.result['readings'][sample['Id']]['mass'],.6355,places=10)
        self.command('move',Tool=sample['Id'],X=0,Z=0)
        self.command('add',Kind='balance');balance=self.workspace['Chemistry']['Tools'][-1]
        self.command('move',Tool=balance['Id'],X=1.25,Z=0)
        self.assertAlmostEqual(self.result['readings'][balance['Id']]['instrument']['value'],.6355,places=10)

    def test_named_solutions_do_not_invent_reactions(self):
        self.empty();self.command('add',Kind='stock',Content='HCl');acid=self.workspace['Chemistry']['Tools'][-1]
        self.command('add',Kind='stock',Content='NaOH');base=self.workspace['Chemistry']['Tools'][-1]
        self.assertAlmostEqual(self.result['readings'][base['Id']]['ph'],13,places=10)
        code,result=http('/api/command',{'Action':'transfer','Workspace':self.workspace,'Tool':acid['Id'],'Target':base['Id'],'Value':10})
        self.assertEqual(code,400);self.assertIn('UnsupportedModel',result['error'])
        self.command('add',Kind='stock',Content='H2SO4');sulfate=self.workspace['Chemistry']['Tools'][-1]
        self.assertIsNone(self.result['readings'][sulfate['Id']]['ph'])

    def test_volumetric_aliquot_and_model_headers(self):
        self.workspace['ChemistryModel']='dilution-v1';self.workspace['ElectricalModel']='resistive-dc-v1'
        self.command('view');self.assertEqual(self.workspace['ChemistryModel'],'dilution-v1')
        self.command('add',Kind='volpipette');pipette=self.workspace['Chemistry']['Tools'][-1]
        self.assertEqual(self.workspace['ChemistryModel'],'ideal-single-solute-v2')
        self.command('transfer',Tool='stock',Target=pipette['Id'],Value=25)
        code,_=http('/api/command',{'Action':'transfer','Workspace':self.workspace,'Tool':pipette['Id'],'Target':'beaker','Value':10})
        self.assertEqual(code,400)
        self.command('transfer',Tool=pipette['Id'],Target='beaker',Value=25)
        self.assertAlmostEqual(self.result['readings']['beaker']['concentration'],2)
        self.workspace['ChemistryModel']='unknown-future-model'
        self.assertEqual(http('/api/command',{'Action':'view','Workspace':self.workspace})[0],400)

    def test_dc_presets_and_power(self):
        self.circuit('series');self.assertAlmostEqual(self.result['readings']['resistor']['branch']['CurrentA'],.0045)
        self.circuit('parallel');self.assertAlmostEqual(self.result['readings']['battery']['branch']['CurrentA'],-.018)
        self.circuit('led');self.assertAlmostEqual(self.result['readings']['extra']['branch']['CurrentA'],7/1010,places=10)
        self.assertAlmostEqual(sum(r['branch']['PowerW'] for r in self.result['readings'].values() if r['branch']),0,places=10)
        self.circuit('capacitor');self.assertEqual(self.result['circuit']['status'],'OpenCircuit')
        self.assertAlmostEqual(self.result['readings']['extra']['charge'],.0009)
        self.circuit('switch-lamp');self.assertAlmostEqual(self.result['readings']['resistor']['branch']['CurrentA'],9/100)
        self.command('mode',Tool='extra',Content='open');self.assertEqual(self.result['circuit']['status'],'OpenCircuit')
        self.command('undo');self.assertAlmostEqual(self.result['readings']['resistor']['branch']['CurrentA'],9/100)

    def test_real_wires_short_protection_and_meter_modes(self):
        self.workspace['ActiveExperiment']='electricity';self.command('view')
        for a,b in [('battery.a','resistor.a'),('resistor.b','battery.b'),('multimeter.a','resistor.a'),('multimeter.b','resistor.b')]:
            self.command('wire',A=a,B=b)
        self.assertAlmostEqual(self.result['readings']['multimeter']['instrument']['value'],9)
        self.command('mode',Tool='multimeter',Content='current');self.assertEqual(self.result['circuit']['status'],'ShortCircuit')
        self.assertIsNone(self.result['readings']['multimeter']['instrument']['value'])
        self.command('undo');self.assertAlmostEqual(self.result['readings']['resistor']['branch']['CurrentA'],.009)
        wire=self.workspace['Electricity']['Wires'][0]['Id'];self.command('unwire',Content=wire)
        self.assertEqual(self.result['circuit']['status'],'OpenCircuit')

    def test_every_catalog_type_and_logic_uses_real_command_validation(self):
        _,catalog=http('/api/catalog')
        for definition in catalog['tools']:
            circuit=definition['Category'] in ['sources','passive','semiconductors','electrical-meters','logic','circuit-reference']
            self.empty('electricity' if circuit else 'chemistry')
            self.command('add',Kind=definition['Kind'])
            tool=self.workspace['Electricity' if circuit else 'Chemistry']['Tools'][0]
            self.assertEqual(tool['Kind'],definition['Kind'])
            if definition['Behavior']=='logic':
                outputs={'and':[False,False,False,True],'or':[False,True,True,True],'not':[True,True,False,False],
                         'nand':[True,True,True,False],'nor':[True,False,False,False],'xor':[False,True,True,False]}
                for bits,expected in zip(['00','01','10','11'],outputs[tool['Kind']]):
                    self.command('mode',Tool=tool['Id'],Content=bits)
                    self.assertEqual(self.result['readings'][tool['Id']]['output'],expected)
                self.assertEqual(http('/api/command',{'Action':'wire','Workspace':self.workspace,'A':tool['Id']+'.a','B':tool['Id']+'.b'})[0],400)

    def test_bounds_history_and_invalid_import_preserve_state(self):
        self.workspace['ActiveExperiment']='electricity'
        for value in range(25):self.command('value',Tool='battery',Value=value%12)
        self.assertEqual(len(self.workspace['ElectricityUndo']),20)
        self.assertEqual(http('/api/command',{'Action':'value','Workspace':self.workspace,'Tool':'battery','Value':-1})[0],400)
        self.assertEqual(http('/api/command',{'Action':'move','Workspace':self.workspace,'Tool':'battery','X':100,'Z':0})[0],400)
        self.assertEqual(http('/api/command',{'Action':'move','Workspace':self.workspace,'Tool':'battery','X':0,'Z':-.3})[0],400)
        invalid=copy.deepcopy(self.workspace);invalid['SchemaVersion']=99
        self.assertEqual(http('/api/command',{'Action':'view','Workspace':invalid})[0],400)
        self.command('view');self.assertEqual(self.workspace['SchemaVersion'],1)


if __name__=='__main__':unittest.main(verbosity=2)
