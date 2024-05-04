using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecoilMacro.Model;
using System.Windows;

namespace RecoilMacro.Helpers
{
    public class PresetManager
    {
        private List<Preset> presets;

        public PresetManager()
        {
            presets = LoadPresets();
        }

        private List<Preset> LoadPresets()
        {
            try
            {
                string filePath = "presets.json";
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<List<Preset>>(json) ?? new List<Preset>();
                }
                return new List<Preset>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during loading presets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Preset>();
            }
        }

        public void SavePresets()
        {
            try
            {
                string filePath = "presets.json";
                string json = JsonConvert.SerializeObject(presets, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during saving presets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Preset> GetPresets()
        {
            return presets;
        }
    }

}
