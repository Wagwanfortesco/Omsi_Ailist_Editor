using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Omsi_Ailist_Editor
{
    public partial class MainWindow : Window
    {
        public static bool backup = false;
        
        public MainWindow()
        {
            InitializeComponent();
            if (ConfigFile.initConfig())
            {
                if (ConfigFile.autoBackup)
                {
                    backup = true;  
                }
            }
            Watermark.Visibility = Visibility.Visible;
            RegInfo = new List<string>();
            BusCountChckList = new List<string>();
            RepaintInfo = new List<string>();
            FleetNumberInfo = new List<string>();
            NewReges = new List<string>();
            NewNos = new List<string>();
            CtiRepaint = new List<string>();
            Depots_List.Visibility = Visibility.Hidden;
            Depots_List.Visibility = Visibility.Hidden;
            Depot_Select_Text.Visibility = Visibility.Hidden;
            Change_Bus_Button.Visibility = Visibility.Hidden;
            Depot_Bus_Info.Visibility = Visibility.Hidden;
            Change_Registration_Button.Visibility = Visibility.Hidden;
            Change_Repaint_Button.Visibility = Visibility.Hidden;
            Add_New_Bus_Button.Visibility = Visibility.Hidden;
            LondonFix londonFix = new LondonFix(this);
        }


        public bool IsFileLoaded { get; set; } = false;
        public string Pfile { get; set; } = string.Empty;
        public string Pbustype { get; set; } = string.Empty;
        public string NewBusFile { get; set; } = string.Empty;
        public string SelectedDepot { get; set; } = string.Empty; 
        public string FleetInfo { get; set; } = string.Empty;
        public List<string> RepaintInfo { get; set; } = null;
        public List<string> RegInfo { get; set; } = null;
        public List<string> FleetNumberInfo { get; set; } = null;
        public List<string> NewReges { get; set; } = null;
        public List<string> NewNos { get; set; } = null;
        public List<string> CtiRepaint { get; set; } = null;
        public List<string> BusCountChckList { get; set; } = null;
        public string BusFileFullPath { get; set; } = string.Empty;
        public bool ManualOrgSelection { get; set; } = false;
        public bool IsLondon { get; set; } = false;
        
        
        private void Load_Ailist_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Ailist File",
                Filter = "Ailist Files (*.cfg)|*.cfg|All Files (*.*)|*.*",
                DefaultExt = "cfg"
            };

            while (true)
            {
                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string selectedFile = openFileDialog.FileName;
                    string filePath = System.IO.Path.GetDirectoryName(selectedFile);

                    if (System.IO.Path.GetExtension(selectedFile) != ".cfg" || !System.IO.Path.GetFileNameWithoutExtension(selectedFile).Contains("ailists"))
                    {
                        MessageBox.Show("File not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    IsFileLoaded = true;
                    // make backup of ailist file
                    string backupFilePath = System.IO.Path.Combine(filePath, $"ailists_backup_{DateTime.Now:yyyyMMdd_HHmmss}.cfg");

                    if (filePath.Contains("London"))
                    {
                        IsLondon = true;
                    }
                    MessageBox.Show("Selected file loaded: " + filePath + "\\" + System.IO.Path.GetFileName(selectedFile), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (backup)
                    {
                        System.IO.File.Copy(selectedFile, backupFilePath, true);
                        Debug.WriteLine($"Backup created at: {backupFilePath}");
                        MessageBox.Show("Backup created at: " + backupFilePath, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    break;
                }
                else
                {
                    break;
                }
            }

            if (IsFileLoaded && IsLondon)
            {// Pass the current MainWindow instance to LondonFix
                LondonFix londonFix = new LondonFix(this);
                Load_Ailist_Button.IsEnabled = false;
                Load_Ailist_Button.Visibility = Visibility.Hidden;
                Debug.WriteLine($"London process");
                londonFix.LondonProcessDepot(openFileDialog.FileName);
            }

            if (IsFileLoaded && IsLondon == false)
            {
                Load_Ailist_Button.IsEnabled = false;
                Load_Ailist_Button.Visibility = Visibility.Hidden;
                ProcessDepot(openFileDialog.FileName);
            }
        }

        public void ProcessDepot(string file)
        {
            using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            using (var sr = new System.IO.StreamReader(fs))
            {
                string line;
                bool inAigroupDepot = false;
                List<string> depotNumbers = new List<string>();

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Equals("[aigroup_depot]", StringComparison.OrdinalIgnoreCase))
                    {
                        inAigroupDepot = true;
                        continue;
                    }

                    if (line.Equals("[end]", StringComparison.OrdinalIgnoreCase) && inAigroupDepot)
                    {
                        inAigroupDepot = false;
                        continue;
                    }

                    if (inAigroupDepot)
                    {
                        depotNumbers.Add(line);
                        inAigroupDepot = false;
                    }
                }


                Display(depotNumbers, file);
                Pfile = file;
            }
        }

        public void Display(List<string> depotNumbers, string file)
        {
            Depots_List.Items.Clear();

            foreach (var number in depotNumbers)
            {
                Depots_List.Items.Add(number);
            }

            Depots_List.Visibility = Visibility.Visible;
            Depot_Select_Text.Visibility = Visibility.Visible;
        }

        private void Depots_List_Click(object sender, MouseButtonEventArgs e)
        {
            FleetNumberInfo.Clear();
            RegInfo.Clear();
            RepaintInfo.Clear();
            if (Depots_List.SelectedItem != null && !IsLondon)
            {
                SelectedDepot = Depots_List.SelectedItem.ToString();
                Depot_Bus_Info.Items.Clear();
                ProcessBussInfo(SelectedDepot, Pfile);
            }

            if (Depots_List.SelectedItems != null && IsLondon)
            {
                SelectedDepot = Depots_List.SelectedItem.ToString();
                Depot_Bus_Info.Items.Clear();
                LondonFix londonFix = new LondonFix(this);
                londonFix.LondonProcessBusinfo(Pfile);
                londonFix.LondonProcessNos(Pfile);
                IsLondon = false;
            }
            else if (Depots_List.SelectedItem == null)
            {
                MessageBox.Show("Please select a depot from the list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private int i = 0;
        private void ProcessBussInfo(string selected, string file)
        {
            using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fs))
                {
                    string line;
                    bool inThecorrectDepot = false;
                    bool inTypeGroup = false;
                    string BusType = string.Empty;
                    if(BusCountChckList.Count > 0)
                    {
                        BusCountChckList.Clear();
                    }
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                        {
                            inTypeGroup = true;
                            continue;
                        }
                            
                        if (line.Equals(selected, StringComparison.OrdinalIgnoreCase))
                        {
                            inThecorrectDepot = true;
                            continue;
                        }

                        if (inTypeGroup && inThecorrectDepot && line.Equals("[aigroup_depot]", StringComparison.OrdinalIgnoreCase))
                        {
                            inThecorrectDepot = false;
                            inTypeGroup = false;
                            continue;
                        }

                        if (inThecorrectDepot && line.Contains("Vehicles"))
                        {
                            Debug.WriteLine(line);
                            BusType = line;
                            BusCountChckList.Add(line);
                            Depot_Bus_Info.Visibility = Visibility.Visible;
                            Depot_Bus_Info.Items.Add(BusType);
                            Debug.WriteLine("Bus added to list");
                            Change_Bus_Button.Visibility = Visibility.Visible;
                            Add_New_Bus_Button.Visibility = Visibility.Visible; 
                            //Change_Fleet_Number_Button.Visibility = Visibility.Visible;
                            //Change_Registration_Button.Visibility = Visibility.Visible;
                            //Change_Repaint_Button.Visibility = Visibility.Visible;
                        }
                        if (inThecorrectDepot == true && inTypeGroup)
                        {
                            
                            if (!line.Contains("Vehicles") && line.Contains('\t'))
                            {
                                //Debug.WriteLine(line);
                                Depot_Bus_Info.Items.Add(line);

                                FleetInfo = line;
                                if (FleetInfo != null)
                                {
                                    try
                                    {
                                        string fleetNumber = FleetInfo.Substring(0, FleetInfo.IndexOf('\t'));
                                        string Reg = FleetInfo.Substring(FleetInfo.IndexOf('\t') + 1, FleetInfo.LastIndexOf('\t') - FleetInfo.IndexOf('\t') - 1);
                                        string repaint = FleetInfo.Substring(FleetInfo.LastIndexOf('\t') + 1);

                                        FleetNumberInfo.Add(fleetNumber);
                                        RegInfo.Add(Reg);
                                        RepaintInfo.Add(repaint);
                                        Debug.WriteLine($"Fleet Number: {fleetNumber}");
                                        Debug.WriteLine($"Reg:{Reg}");
                                        Debug.WriteLine($"Repaint: {repaint}");
                                    }
                                    catch (Exception ex)
                                    {
                                        if (i < 1)
                                        {
                                            MessageBox.Show($"{ex.Message}: This may be due to an issue getting the fleet number, reg or repaint info. This may affect the edit of this depot", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            i++;
                                        }
                                    }
                                    
                                }
                                if(BusCountChckList.Count > 1)
                                {
                                    IsLondon = true;
                                    BusCountChckList.Clear();
                                    break;
                                }
                                Debug.WriteLine($"FleetNumberInfo Count: {FleetNumberInfo.Count}");
                                Debug.WriteLine($"RegInfo Count: {RegInfo.Count}");
                                Debug.WriteLine($"RepaintInfo Count: {RepaintInfo.Count}");
                            }
                        }

                    }
                }
            }
        }


        private void Bus_List_Click(object sender, MouseButtonEventArgs e)
        {
            Depot_Bus_Info.Focus();
            if (Depot_Bus_Info.SelectedItem != null && Depot_Bus_Info.SelectedItems.Cast<object>().Any(item => item.ToString().StartsWith("Vehicles")))
            {
                Pbustype = Depot_Bus_Info.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Please select a bus type from the list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Change_Bus_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Pbustype))
            {
                MessageBox.Show("Please select a bus type from the list you need to double click the bus.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Bus File",
                Filter = "Bus Files (*.bus)|*.bus|All Files (*.*)|*.*",
                DefaultExt = "bus"
            };

            while (true)
            {
                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    var filePath = openFileDialog.FileName;
                    BusFileFullPath = filePath;
                    if (System.IO.Path.GetExtension(filePath) != ".bus")
                    {
                        MessageBox.Show("File not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    var relativePath = filePath.Contains("Vehicles")
                        ? filePath.Substring(filePath.IndexOf("Vehicles", StringComparison.Ordinal))
                        : "Vehicles/" + System.IO.Path.GetFileName(filePath);

                    MessageBox.Show("Selected file loaded: " + relativePath + "Program may freeze for 1-3 seconds", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    NewBusFile = relativePath;
                    if (relativePath.Contains("Renown"))
                    {
                        ManualOrgSelection = true;
                        MessageBox.Show("Renown detected, please select the Reg and Nos files manually also note that the default org files in the Renown may be to short", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    if (IsLondon)
                    {
                        LondonFix londonFix = new LondonFix(this);
                        londonFix.ChangeBusLondon();
                    }
                    else
                    {
                        ChangeBus(NewBusFile, Pfile, filePath);
                    }
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        private void ChangeBus(string newBusFile, string file, string realFiles)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(file);
                bool inCorrectDepot = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line.Equals("[aigroup_depot]", StringComparison.OrdinalIgnoreCase))
                    {
                        inCorrectDepot = false; // Reset the flag when encountering a new depot section
                        continue;
                    }

                    if (line.Equals(SelectedDepot, StringComparison.OrdinalIgnoreCase))
                    {
                        inCorrectDepot = true;
                        continue;
                    }

                    if (line.Equals("[end]", StringComparison.OrdinalIgnoreCase))
                    {
                        inCorrectDepot = false;
                        continue;
                    }

                    if (inCorrectDepot && lines[i].Contains("Vehicles")) // don't trim here to preserve format
                    {
                        Debug.WriteLine($"Replacing line: {lines[i]} ➜ {newBusFile}");
                        lines[i] = newBusFile;
                        break; 
                    }
                }

                System.IO.File.WriteAllLines(file, lines);
                //MessageBox.Show("Bus type successfully updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CheckAndReadRegesFile(realFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while changing the bus: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CheckAndReadRegesFile(string path)
        {
            // Define the directory to search for the files
            string directoryPath = System.IO.Path.GetDirectoryName(path);

            Debug.WriteLine($"Reg length: {RegInfo.Count}");

            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                MessageBox.Show("Directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Search for files with "reges" in their name and ".org" extension
            var regesFiles = Directory.GetFiles(directoryPath, "Reg*.org")
                .Concat(Directory.GetFiles(directoryPath, "Registrations*.org"))
                .ToArray();

            if (regesFiles.Length == 0 || ManualOrgSelection)
            {
                if (ManualOrgSelection == false)
                {
                    MessageBox.Show("No 'Reges' files found. Please select a file manually.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Reg File",
                    Filter = "Reg Files (*.org)|*.org|All Files (*.*)|*.*",
                    DefaultExt = "org"
                };

                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    regesFiles = new[] { openFileDialog.FileName };
                }
                else
                {
                    MessageBox.Show("No file selected. Operation canceled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Open the first file
            string firstRegesFile = regesFiles[0];
            try
            {
                var allLines = File.ReadAllLines(firstRegesFile).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                Random random = new Random();

                for (int i = 0; i < RegInfo.Count; i++)
                {
                    if (allLines.Count == 0)
                        break;

                    // Pick a random line
                    int randomIndex = random.Next(allLines.Count);
                    string randomReg = allLines[randomIndex];
                    NewReges.Add(randomReg);

                    // Remove the selected line to avoid duplicates
                    allLines.RemoveAt(randomIndex);

                    Debug.WriteLine($"Randomly selected Reg: {randomReg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CheckAndReadNosFile(path); 


        }
        public void CheckAndReadNosFile(string path)
        {
            // Define the directory to search for the files  
            string directoryPath = System.IO.Path.GetDirectoryName(path);

            Debug.WriteLine($"Nos length: {FleetNumberInfo.Count}");

            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                MessageBox.Show("Directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var nosFiles = Directory.GetFiles(directoryPath, "Nos*.org")
                .Concat(Directory.GetFiles(directoryPath, "Numbers*.org"))
                .Concat(Directory.GetFiles(directoryPath, "Numbers_*.org"))
                .Concat(Directory.GetFiles(directoryPath, "Nos_*.org"))
                .ToArray();

            if (nosFiles.Length == 0 || ManualOrgSelection)
            {
                if (ManualOrgSelection == false)
                {
                    MessageBox.Show("No 'Nos' files found. Please select a file manually.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Nos File",
                    Filter = "Nos Files (*.org)|*.org|All Files (*.*)|*.*",
                    DefaultExt = "org"
                };

                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    nosFiles = new[] { openFileDialog.FileName };
                }
                else
                {
                    MessageBox.Show("No file selected. Operation canceled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string firstNosFile = nosFiles[0];
            try
            {
                var allLines = File.ReadAllLines(firstNosFile).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                Random random = new Random();

                for (int i = 0; i < FleetNumberInfo.Count; i++)
                {
                    if (allLines.Count == 0)
                        break;

                    while (NewNos.Count != FleetNumberInfo.Count)
                    {
                        // Pick a random line  
                        int randomIndex = random.Next(allLines.Count);
                        string randomNos = allLines[randomIndex];
                        if (!NewNos.Contains(randomNos))   
                        {
                            NewNos.Add(randomNos);
                            allLines.RemoveAt(randomIndex); 
                            Debug.WriteLine($"Randomly selected Nos: {randomNos}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!IsLondon)
            {
                ChangeReg(Pfile, NewReges); 
            }
            else if (IsLondon)
            {
                ChangeFleetNumber(Pfile);         
            }
        }

        public void ChangeReg(string path, List<string> newNumbs)
        {

            var lines = System.IO.File.ReadAllLines(path).ToList();
            bool inCorrectDepot = false;
            bool Inairgroup = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    Inairgroup = true;
                    continue;
                }

                if (line.Equals(SelectedDepot, StringComparison.OrdinalIgnoreCase)) 
                {
                    inCorrectDepot = true;
                    continue;
                }

                if (line.Equals("[end]", StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = false;
                    continue;
                }

                if (inCorrectDepot)
                {
                    foreach (var reg in RegInfo)
                    {
                        if (line.Contains(reg))
                        {
                            Debug.WriteLine($"Replacing line: {line} ➜ {NewReges[RegInfo.IndexOf(reg)]}");
                            lines[i] = line.Replace(reg, NewReges[RegInfo.IndexOf(reg)]);
                            break; 
                        }

                    }

                }

            }
            System.IO.File.WriteAllLines(path, lines);
            ChangeFleetNumber(path);
        }
        public void ChangeFleetNumber(string path)
        {
            if (IsLondon)
            {
                if (RegInfo.Count != 0 && RegInfo.All(item => !string.IsNullOrEmpty(item)))
                {
                    // This is a bodge job but it is how i have done it
                }
                else
                {
                    LondonFix londonFix = new LondonFix(this);
                    londonFix.DeleteLondonReg();
                }
            }

            var lines = System.IO.File.ReadAllLines(path).ToList();
            bool inCorrectDepot = false;
            bool inAirgroup = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    inAirgroup = true;
                    continue;
                }

                if (line.Equals(SelectedDepot, StringComparison.OrdinalIgnoreCase)) 
                {
                    inCorrectDepot = true;
                    continue;
                }

                if (line.Equals("[end]", StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = false;
                    inAirgroup = false;
                    continue;
                }

                if (inCorrectDepot == true)
                {
                    foreach (var number in FleetNumberInfo)
                    {
                        if (line.Contains(number))
                        {
                            int index = FleetNumberInfo.IndexOf(number); 
                            if (index >= 0 && index < NewNos.Count) 
                            {
                                Debug.WriteLine($"Replacing line: {line} ➜ {NewNos[index]}");
                                lines[i] = line.Replace(number, NewNos[index]);
                            }
                            else
                            {
                                Debug.WriteLine($"Index out of range for number: {number}");
                            }
                            break;   
                        }
                    }
                }
            }

            System.IO.File.WriteAllLines(path, lines);
            FleetNumberInfo.Clear(); 
            RegInfo.Clear(); 
            NewNos.Clear(); 
            NewReges.Clear(); 
            //RepaintInfo.Clear(); 
            Debug.WriteLine($"FleetNumberInfo Count after clear: {FleetNumberInfo.Count}");
            Debug.WriteLine($"RegInfo Count after clear: {RegInfo.Count}");
            Debug.WriteLine($"NewNos Count after clear: {NewNos.Count}");
            Debug.WriteLine($"NewReges Count after clear: {NewReges.Count}");
            Debug.WriteLine($"RepaintInfo Count after clear: {RepaintInfo.Count}");
            ChangeRepaint(path);
        }
        
        public void ChangeRepaint(string CfgPath)
        {
            List<string> repaintFileLines = new List<string>();
            string repaintfile = null;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = $"Select Repaint File for: {System.IO.Path.GetFileName(NewBusFile.ToString())}",
                Filter = "Repaint Files (*.cti)|*.cti|All Files (*.*)|*.*",
                DefaultExt = "org"
            };

            while (true)
            {
                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string selectedFile = openFileDialog.FileName;
                    string filePath = System.IO.Path.GetDirectoryName(selectedFile);

                    if (System.IO.Path.GetExtension(selectedFile) != ".cti")
                    {
                        MessageBox.Show("File not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    MessageBox.Show("Selected file loaded: " + filePath, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFileLoaded = true;
                    repaintfile = selectedFile;
                    break;
                }
                else
                {
                    return;
                }
            }
            repaintFileLines = System.IO.File.ReadAllLines(repaintfile).ToList();

            Debug.WriteLine($"Repaint file length: {repaintFileLines.Count}");
            for (int i = 0; i < repaintFileLines.Count; i++)
            {
                var line = repaintFileLines[i].Trim();
                bool inItem1 = false || line.Equals("[item]", StringComparison.OrdinalIgnoreCase);
                if (inItem1 == true)
                {
                    if (i + 1 < repaintFileLines.Count)
                    {
                        string repaintName = repaintFileLines[i + 1].Trim();
                        Debug.WriteLine($"Repaint: {repaintName}");

                        if (!CtiRepaint.Contains(repaintName))
                        {
                            CtiRepaint.Add(repaintName);
                            Debug.WriteLine($"Adding repaint: {repaintName}");
                        }
                        inItem1 = false;
                    }
                }
            }
            RepaintSelect repaintSelect = new RepaintSelect(CtiRepaint);
            repaintSelect.ShowDialog();
            repaintSelect.Focus();
            var selectedRepaint = repaintSelect.SelectedRepaint;
            while (true)
            {
                if (selectedRepaint != null)
                {
                    break;
                }
            }

            var lines = System.IO.File.ReadAllLines(CfgPath).ToList();
            bool inCorrectDepot = false;
            bool inCorrectAigroup = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals(SelectedDepot, StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = true;
                    continue;
                }

                if (line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectAigroup = true;
                    continue;
                }

                if (line.Equals("[end]", StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = false;
                    inCorrectAigroup = false;
                    continue;
                }

                if (inCorrectDepot && inCorrectAigroup)
                {
                    foreach (var Repaint in RepaintInfo)
                    {
                        if (line.Contains(Repaint))
                        {
                            // Use a more precise match to replace the entire repaint name
                            string fullRepaintName = line.Substring(line.LastIndexOf('\t') + 1); // Extract the repaint name from the line
                            if (fullRepaintName.Equals(Repaint, StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.WriteLine($"Replacing line: {line} ➜ {selectedRepaint}");
                                lines[i] = line.Replace(fullRepaintName, selectedRepaint);
                            }
                            else
                            {
                                Debug.WriteLine($"Skipping line: {line} (Repaint mismatch)");
                            }
                        }
                    }
                }
            }

            System.IO.File.WriteAllLines(CfgPath, lines);
            Depot_Bus_Info.Items.Clear();
            repaintFileLines.Clear();
            MessageBox.Show("Depot updated!!! (Dobule click on the selected depot to refresh the list)!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            CtiRepaint.Clear();
        }


        private void add_new_bus_type_click(object sender, RoutedEventArgs e)
        {
            // What a shit show of a function
            string selectedBus = "";
            string newNosFile = "";
            string newRegFile = "";
            string repaintFile = "";
            int ammount = 3;
            List<string> newNos = new List<string>();
            List<string> newRegs = new List<string>();
            string newRepaint = "";
            if (SelectedDepot != null)
            {
                Debug.WriteLine($"(caller = add_new_bus_type_click method) SelectedDepot: {SelectedDepot}");
                
                {
                    // code block to select bus file
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Title = "Select Bus File",
                        Filter = "Bus Files (*.bus)|*.bus|All Files (*.*)|*.*",
                        DefaultExt = "bus"
                    };
                    
                    bool? result = openFileDialog.ShowDialog();
                    if (result == true)
                    {
                        var filePath = openFileDialog.FileName;
                        BusFileFullPath = filePath;
                        if (System.IO.Path.GetExtension(filePath) != ".bus")
                        {
                            MessageBox.Show("File not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        var relativePath = filePath.Contains("Vehicles")
                            ? filePath.Substring(filePath.IndexOf("Vehicles", StringComparison.Ordinal))
                            : "Vehicles/" + System.IO.Path.GetFileName(filePath);

                        NewBusFile = relativePath;
                        selectedBus = relativePath;
                        Debug.WriteLine($"$(New bus selected: {selectedBus})");
                    }
                }
                
                {
                    // code block to select nos file
                    OpenFileDialog openFileDialog2 = new OpenFileDialog
                    {
                        Title = "Select Nos File",
                        Filter = "Nos Files (*.org)|*.org|All Files (*.*)|*.*",
                        DefaultExt = "org"
                    };
                    bool? result = openFileDialog2.ShowDialog();
                    if (result == true)
                    {
                        var filePath = openFileDialog2.FileName;
                        if (System.IO.Path.GetExtension(filePath) != ".org")
                        {
                        }
                        newNosFile = filePath;
                        Debug.WriteLine($"New Nos file: {newNosFile}");
                    }
                }
                
                {
                    // code block to parse the nos file and select fleet numbers
                    List<string> lines = new List<string>();
                    lines = System.IO.File.ReadAllLines(newNosFile).ToList();
                    Random random = new Random();
                    for (int i = 0; i < ammount; i++)
                    {
                        newNos.Add(lines[random.Next(lines.Count)]);
                        Debug.WriteLine($"New Nos added: {newNos[i]}");
                        if (string.IsNullOrWhiteSpace(newNos[i]))
                        {
                                i--;
                        }
                    }
                }

                {
                    // code block to select reg file
                    OpenFileDialog openFileDialog3 = new OpenFileDialog
                    {
                        Title = "Select Reg File",
                        Filter = "Reg Files (*.org)|*.org|All Files (*.*)|*.*",
                        DefaultExt = "org"
                    };
                    bool? result = openFileDialog3.ShowDialog();
                    if (result == true)
                    {
                        var filePath = openFileDialog3.FileName;
                        if (System.IO.Path.GetExtension(filePath) != ".org")
                        {
                        }
                        newRegFile = filePath;
                        Debug.WriteLine($"New Reg file: {newRegFile}");
                    }
                }
                {
                    // code block to parse the reg file and select registrations
                    List<string> lines = new List<string>();
                    lines = System.IO.File.ReadAllLines(newRegFile).ToList();
                    Random random = new Random();
                    for (int i = 0; i < ammount; i++)
                    {
                        newRegs.Add(lines[random.Next(lines.Count)]);
                        Debug.WriteLine($"New Reg added: {newRegs[i]}");
                        if (string.IsNullOrWhiteSpace(newRegs[i]))
                        {
                            i--;
                        }
                    }
                }
                {
                    // code block to select repaint file
                    OpenFileDialog openFileDialog4 = new OpenFileDialog
                    {
                        Title = $"Select Repaint File for: {System.IO.Path.GetFileName(NewBusFile.ToString())}",
                        Filter = "Repaint Files (*.cti)|*.cti|All Files (*.*)|*.*",
                        DefaultExt = "cti"
                    };
                    bool? result = openFileDialog4.ShowDialog();
                    if (result == true)
                    {
                        var filepath = openFileDialog4.FileName;
                        if (System.IO.Path.GetExtension(filepath) != ".cti")
                        {
                            
                        }
                        repaintFile = filepath;
                        Debug.WriteLine($"New Repaint file: {repaintFile}");
                    }
                }
                {
                    // code block to parse the repaint file and select repaints
                    List<string> lines = System.IO.File.ReadAllLines(repaintFile).ToList();

                    Debug.WriteLine($"Repaint file length: {lines.Count}");
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i].Trim();
                        bool inItem1 = false || line.Equals("[item]", StringComparison.OrdinalIgnoreCase);
                        if (inItem1 == true)
                        {
                            if (i + 1 < lines.Count)
                            {
                                string repaintName = lines[i + 1].Trim();
                                Debug.WriteLine($"Repaint: {repaintName}");

                                if (!CtiRepaint.Contains(repaintName))
                                {
                                    CtiRepaint.Add(repaintName);
                                    Debug.WriteLine($"Adding repaint: {repaintName}");
                                }
                                inItem1 = false;
                            }
                        }
                    }
                    RepaintSelect repaintSelect = new RepaintSelect(CtiRepaint);
                    repaintSelect.ShowDialog();
                    repaintSelect.Focus();
                    newRepaint = repaintSelect.SelectedRepaint;
                }
                
               
                if(AddNewBusType(Pfile, selectedBus, newNos, newRegs, newRepaint, SelectedDepot,ammount))
                {
                    MessageBox.Show("New bus type added to depot!!! (Dobule click on the selected depot to refresh the list)!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Depot_Bus_Info.Items.Clear();
                    CtiRepaint.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to add new bus type to depot.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
        
        public static bool AddNewBusType(string cfgPath, string newBusType, List<string> numbers, List<string> reges, string repaint, string selectedDepot,int ammount)
        {
            try
            {
                List<string> lines = System.IO.File.ReadAllLines(cfgPath).ToList();
                List<string> newLines = new List<string>();
                bool inCorrectDepot = false;
                bool inAigroup = false;
                string busType = newBusType;
                int regAndNosIndex = 0;
                
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Trim().Equals(selectedDepot))
                    {
                        inCorrectDepot = true;;
                    }
                    else if (lines[i].Trim().Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                    {
                        inAigroup = true;
                    }

                    if (inCorrectDepot && inAigroup && lines[i].Trim().Equals("[end]",StringComparison.OrdinalIgnoreCase))
                    {
                        newLines.Add(lines[i]);
                        newLines.Add("\n");
                        newLines.Add("[aigroup_depot_typgroup_2]");
                        newLines.Add(busType);
                        
                        for (int j = 0; j < ammount && regAndNosIndex < numbers.Count && regAndNosIndex < reges.Count; j++, regAndNosIndex++)
                        {
                            newLines.Add($"{numbers[regAndNosIndex]}\t{reges[regAndNosIndex]}\t{repaint}");
                            regAndNosIndex++;
                        }
                        newLines.Add("[end]");
                        inCorrectDepot = false;
                        inAigroup = false;
                        continue;
                    }
                    newLines.Add(lines[i].Trim());
                }
                System.IO.File.WriteAllLines(cfgPath, newLines);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
    
    
}

