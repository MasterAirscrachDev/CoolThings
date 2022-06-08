//SLA System V3
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
//serialised class to save
struct CerData{ public int[] SInt; public float[] SFloat; public bool[] SBool; public string[] SString, SIntNames, SFloatNames, SBoolNames, SStringNames; }
public class SaveFile { //the editable class
    public bool debug;
    public List<int> IntList = new List<int>(); public List<float> FloatList = new List<float>(); public List<bool> BoolList = new List<bool>(); public List<string> StringList = new List<string>(), IntNames = new List<string>(), FloatNames = new List<string>(), BoolNames = new List<string>(), StringNames = new List<string>();
    public string projectName, studioName;
    //0 permission cannot overwrite
    //1 permission can update data
    //2 permission can overwrite data
    public void SetInt(int index, int value, string name, int permission = 0) {
        if(IntList.Count - 1 < index){
            for(int i = IntList.Count; i <= index; i++){ IntList.Add(0); IntNames.Add(""); }
        }
        if (permission == 0 && IntNames[index] == null) { IntList[index] = value; IntNames[index] = name; }
        else if (permission == 1 && IntNames[index] == name) { IntList[index] = value; }
        else if (permission == 2) { IntList[index] = value; IntNames[index] = name; }
    }
    public void SetFloat(int index, float value, string name, int permission = 0) {
        if(FloatList.Count - 1 < index){
            for(int i = FloatList.Count; i <= index; i++){ FloatList.Add(0); FloatNames.Add(""); }
        }
        if (permission == 0 && FloatNames[index] == null) { FloatList[index] = value; FloatNames[index] = name; }
        else if (permission == 1 && FloatNames[index] == name) { FloatList[index] = value; }
        else if (permission == 2) { FloatList[index] = value; FloatNames[index] = name; }
    }
    public void SetBool(int index, bool value, string name, int permission = 0) {
        if(BoolList.Count - 1 < index){
            for(int i = BoolList.Count; i <= index; i++){ BoolList.Add(false); BoolNames.Add(""); }
        }
        if (permission == 0 && BoolNames[index] == null) { BoolList[index] = value; BoolNames[index] = name; }
        else if (permission == 1 && BoolNames[index] == name) { BoolList[index] = value; }
        else if (permission == 2) { BoolList[index] = value; BoolNames[index] = name; }
    }
    public void SetString(int index, string value, string name, int permission = 0) {
        if(StringList.Count - 1 < index){
            for(int i = StringList.Count; i <= index; i++){ StringList.Add(""); StringNames.Add(""); }
        }
        if (permission == 0 && StringNames[index] == null) { StringList[index] = value; StringNames[index] = name; }
        else if (permission == 1 && StringNames[index] == name) { StringList[index] = value; }
        else if (permission == 2) { StringList[index] = value; StringNames[index] = name; }
    }
    //configure class
    public void Setup(bool debugfile, string projectname, string studioname) { debug = debugfile; projectName = projectname; studioName = studioname; }
}
public class SLASystem
{
    public bool working = false; //dont quit while working
    string location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //this might need to be changed to be compatable with non-windows platforms
    JsonSerializer serializer = new JsonSerializer(); // the json maker thingy
    //This Handles the encryption
    string EncDecProcess(string jsonIn, string key) { string xorstring = "", input = jsonIn, enckey = key; for (int i = 0; i < input.Length; i++) { xorstring += (char)(input[i] ^ enckey[i % enckey.Length]); } return xorstring; }
    //https://www.youtube.com/watch?v=XVzllHpJktg also this convert a SaveFile to a CerData
    CerData SaveToCereal(SaveFile save){
        CerData data; data.SInt = save.IntList.ToArray(); data.SFloat = save.FloatList.ToArray(); data.SBool = save.BoolList.ToArray(); data.SString = save.StringList.ToArray();
        data.SIntNames = save.IntNames.ToArray(); data.SFloatNames = save.FloatNames.ToArray(); data.SBoolNames = save.BoolNames.ToArray(); data.SStringNames = save.StringNames.ToArray();
        return data;
    }
    // this does it the other way
    SaveFile CerealToSave(CerData crunchy){
        SaveFile save = new SaveFile();
        save.IntList.AddRange(crunchy.SInt); save.FloatList.AddRange(crunchy.SFloat); save.BoolList.AddRange(crunchy.SBool); save.StringList.AddRange(crunchy.SString);
        save.IntNames.AddRange(crunchy.SIntNames); save.FloatNames.AddRange(crunchy.SFloatNames); save.BoolNames.AddRange(crunchy.SBoolNames); save.StringNames.AddRange(crunchy.SStringNames);
        return save;
    }
    //Save A SaveFile
    public void Save(string filename, SaveFile save, string encryptKey = "UseDefaultKey", string subfolder = null){
        //check if the data is valid
        if(!string.IsNullOrEmpty(save.projectName) && !string.IsNullOrEmpty(save.studioName)){ working = true;
            //does the folder exist, if not create it
            if (File.Exists($"{location}\\{save.studioName}\\{save.projectName}\\{subfolder}{filename}.dat")) {}
            else if (subfolder != null) { Directory.CreateDirectory($"{location}\\{save.studioName}\\{save.projectName}\\{subfolder}"); }
            else { Directory.CreateDirectory($"{location}\\{save.studioName}\\{save.projectName}"); }
            //create the files
            StreamWriter savedata = File.CreateText($"{location}\\{save.studioName}\\{save.projectName}\\{subfolder}{filename}.dat"); CerData data = new CerData();
            StreamWriter backup = File.CreateText($"{location}\\{save.studioName}\\{save.projectName}\\{subfolder}{filename}.datbak"); string json = null;
            //convert to CerData
            data = SaveToCereal(save);
            //serialize and/or encrypt the data
            if(string.IsNullOrEmpty(encryptKey)){ json = JsonConvert.SerializeObject(data, Formatting.Indented); } //open data
            else if(encryptKey == "UseDefaultKey"){ string newkey = ""; //uses an automatic encryption key
                for (int i = 0; i < filename.Length; i++) {  try{newkey += save.projectName[i];} catch{} try{newkey += save.studioName[i];} catch{} }
                json = JsonConvert.SerializeObject(data, Formatting.None); json = EncDecProcess(json, newkey);
            } //uses a manual key
            else{ json = JsonConvert.SerializeObject(data, Formatting.None); json = EncDecProcess(json, encryptKey); }
            //write and close the files
            savedata.Write(json); savedata.Close(); backup.Write(json); backup.Close();
            if(save.debug){ Console.WriteLine("Data backed up");} working = false;
        } else{ Console.WriteLine("Project or Studio name is empty!"); }
    }
    //Load A Save File
    public SaveFile Load(string filename, SaveFile savein, string encryptKey = "UseDefaultKey", string subfolder = null){
        SaveFile save = new SaveFile(); string json = null; CerData data = new CerData();
        //check if the input savefile is valid
        if (!string.IsNullOrEmpty(savein.projectName) && !string.IsNullOrEmpty(savein.studioName)){ working = true;
            if (File.Exists($"{location}\\{savein.studioName}\\{savein.projectName}\\{subfolder}{filename}.dat")){
                //get that data
                json = File.ReadAllText($"{location}\\{savein.studioName}\\{savein.projectName}\\{subfolder}{filename}.dat");
                //try a simple convert
                TextReader reader = new StringReader(json);
                try { data = (CerData)serializer.Deserialize(reader, typeof(CerData)); }
                catch {//use complex convert
                    //decrypt the data
                    if (encryptKey == "UseDefaultKey") {
                        string newkey = ""; //creates the key for decrytion
                        for (int i = 0; i < filename.Length; i++) { try { newkey += savein.projectName[i]; } catch { } try { newkey += savein.studioName[i]; } catch { } }
                        //decrypt and convert to CerData
                        json = EncDecProcess(json, newkey); reader = new StringReader(json); data = (CerData)serializer.Deserialize(reader, typeof(CerData));
                    }//the same, but with a manual key
                    else { json = EncDecProcess(json, encryptKey); reader = new StringReader(json); data = (CerData)serializer.Deserialize(reader, typeof(CerData)); }
                }
                //convert the data to a SaveFile format and return it
                save = CerealToSave(data);
                save.debug = savein.debug; save.projectName = savein.projectName; save.studioName = savein.studioName;
                if (save.debug) { Console.WriteLine("Data loaded"); } return save;
            }
            //if the file was missing check for the backup
            else if (File.Exists($"{location}\\{savein.studioName}\\{savein.projectName}\\{subfolder}{filename}.datbak")){
                //get backup
                json = File.ReadAllText($"{location}\\{savein.studioName}\\{savein.projectName}\\{subfolder}{filename}.datbak");
                //try a simple convert
                TextReader reader = new StringReader(json);
                try { data = (CerData)serializer.Deserialize(reader, typeof(CerData)); }
                catch {//use complex convert
                    //decrypt the data
                    if (encryptKey == "UseDefaultKey") {
                        string newkey = ""; //creates the key for decrytion
                        for (int i = 0; i < filename.Length; i++) { try { newkey += savein.projectName[i]; } catch { } try { newkey += savein.studioName[i]; } catch { } }
                        //decrypt and convert to CerData
                        json = EncDecProcess(json, newkey); reader = new StringReader(json); data = (CerData)serializer.Deserialize(reader, typeof(CerData));
                    }//the same, but with a manual key
                    else { json = EncDecProcess(json, encryptKey); reader = new StringReader(json); data = (CerData)serializer.Deserialize(reader, typeof(CerData)); }
                }
                //convert the data to a SaveFile format and return it
                save = CerealToSave(data); save.debug = savein.debug; save.projectName = savein.projectName; save.studioName = savein.studioName;
                if (save.debug) { Console.WriteLine("Data recovered"); }
                //re-write the save file
                Save(filename,save, encryptKey, subfolder); working = false; return save;
            }
            else if (savein.debug) { Console.WriteLine("There is no save data!"); }
            working = false; return null;
        } //if the input SaveFile was invalid
        else { Console.WriteLine("Project or Studio name is not set!"); } return null; 
    }
}