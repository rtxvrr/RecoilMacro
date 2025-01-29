using Newtonsoft.Json;
using RecoilMacro.Model;
using System.IO;
using System.Windows;

namespace RecoilMacro.Helpers
{
    public class PresetManager
    {
        List<Preset> presets;

        public PresetManager()
        {
            presets = LoadPresets();
        }

        List<Preset> LoadPresets()
        {
            try
            {
                if (File.Exists("presets.json"))
                {
                    var json = File.ReadAllText("presets.json");
                    return JsonConvert.DeserializeObject<List<Preset>>(json) ?? new List<Preset>();
                }
                return new List<Preset>();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error during loading presets: {ex.Message}");
                return new List<Preset>();
            }
        }

        public void SavePresets()
        {
            try
            {
                var json = JsonConvert.SerializeObject(presets, Formatting.Indented);
                File.WriteAllText("presets.json", json);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error during saving presets: {ex.Message}");
            }
        }

        public List<Preset> GetPresets()
        {
            return presets;
        }
    }
}
