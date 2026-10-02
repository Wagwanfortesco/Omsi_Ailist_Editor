using Omsi_Ailist_Editor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System;
using System.IO;

public class LondonFix
{  /// <summary>
   /// Just general multiple buses in depot handling now
   /// </summary>
    public List<string> Bustypes { get; private set; } = new List<string>();

    public string FleetInfoLines = null;
    public string SelectedBus = null;
    public string NewBus = null;

    private readonly MainWindow _mainWindow;

    public LondonFix(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public void LondonProcessDepot(string file)
    {
        using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
        using (var sr = new System.IO.StreamReader(fs))
        {
            string line;
            bool inAigroupDepot = false;
            int ammount = 1;
            List<string> depotNumbers = new List<string>();

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Equals("[aigroup_depot]", StringComparison.OrdinalIgnoreCase))
                {
                    inAigroupDepot = true;
                    continue;
                }
                if (inAigroupDepot)
                {
                    depotNumbers.Add(line);
                    inAigroupDepot = false;
                }
            }

            // Use the existing MainWindow instance to update the UI
            _mainWindow.Display(depotNumbers, file);
            _mainWindow.Pfile = file;
        }
    }

    public void LondonProcessBusinfo(string file)
    {
        Bustypes.Clear(); // Clear previous state

        using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
        using (System.IO.StreamReader sr = new System.IO.StreamReader(fs))
        {
            string line;
            bool inAigroupType = false;
            bool inCorrectDepot = false;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    inAigroupType = true;
                }
                if (line.Equals(_mainWindow.SelectedDepot, StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = true;
                }
                if (inAigroupType && inCorrectDepot && line.Equals("[aigroup_depot]", StringComparison.OrdinalIgnoreCase))
                {
                    inAigroupType = false;
                    inCorrectDepot = false;
                }
                if (inAigroupType && inCorrectDepot)
                {
                    if (line.IndexOf("vehicles", StringComparison.OrdinalIgnoreCase) >= 0 && !Bustypes.Contains(line))
                    {
                        Bustypes.Add(line); // Add only if not already present
                        Debug.WriteLine(line);
                    }
                }
            }
        }

        // Show the Multiple Bus Select window if there are multiple bus types
        if (Bustypes.Count > 1)
        {
            Multiple_Bus_In_Depot_Select bus_In_Depot_Select = new Multiple_Bus_In_Depot_Select(Bustypes);
            bus_In_Depot_Select.ShowDialog();
            bus_In_Depot_Select.Focus();
            while (true)
            {
                try
                {
                    if (bus_In_Depot_Select.IsSelectionMade)
                    {
                        _mainWindow.Pbustype = bus_In_Depot_Select.Bus_Select_Box.SelectedItem.ToString();
                        break;
                    }
                    else
                    {
                        MessageBox.Show("No bus selected, defaulting to the first in the list.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
        }
    }



    List<string> added = new List<string>();

    public void LondonProcessNos(string file)
    {
        added.Clear(); // Clear previous state

        using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
        using (System.IO.StreamReader sr = new System.IO.StreamReader(fs))
        {
            string line;
            bool inAigroupType = false;
            bool inCorrectDepot = false;
            bool correctBusType = false;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Equals(_mainWindow.SelectedDepot, StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = true;
                    continue;
                }
                if (inCorrectDepot && line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    inAigroupType = true;
                    continue;
                }
                if (inAigroupType && inCorrectDepot)
                {
                    if (line.IndexOf(_mainWindow.Pbustype, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        correctBusType = true;
                        _mainWindow.Depot_Bus_Info.Visibility = Visibility.Visible;

                        if (!_mainWindow.Depot_Bus_Info.Items.Contains(line) && !line.Equals("[end]")) // Prevent duplicates
                        {
                            _mainWindow.Depot_Bus_Info.Items.Add(line);
                        }

                        _mainWindow.Change_Bus_Button.Visibility = Visibility.Visible;
                    }
                    if (line.IndexOf("[end]", StringComparison.OrdinalIgnoreCase) >= 0 && correctBusType)
                    {
                        correctBusType = false;
                        inAigroupType = false;
                        break;
                    }
                    if (line.IndexOf("vehicles", StringComparison.OrdinalIgnoreCase) < 0 && line.Contains('\t') && correctBusType)
                    {
                        FleetInfoLines = line;

                        if (!_mainWindow.Depot_Bus_Info.Items.Contains(line)) // Prevent duplicates
                        {
                            _mainWindow.Depot_Bus_Info.Items.Add(line);
                            added.Add(line);
                        }

                        Debug.WriteLine(line);
                        if (FleetInfoLines != null)
                        {
                            string fleetNumber = FleetInfoLines?.Substring(0, FleetInfoLines.IndexOf('\t'));
                            string Reg = FleetInfoLines?.Substring(FleetInfoLines.IndexOf('\t') + 1, FleetInfoLines.LastIndexOf('\t') - FleetInfoLines.IndexOf('\t') - 1);
                            string repaint = FleetInfoLines?.Substring(FleetInfoLines.LastIndexOf('\t') + 1);
                            Debug.WriteLine($"Fleet Number: {fleetNumber}");
                            Debug.WriteLine($"Reg: {Reg}");
                            Debug.WriteLine($"Repaint: {repaint}");
                            _mainWindow.FleetNumberInfo.Add(fleetNumber);
                            _mainWindow.RegInfo.Add(Reg);
                            _mainWindow.RepaintInfo.Add(repaint);
                        }
                    }
                }
            }
        }
    }


    public void ChangeBusLondon()
    {
        try
        {
            var lines = System.IO.File.ReadAllLines(_mainWindow.Pfile);
            bool inAigroupType = false;
            bool inCorrectDepot = false;
            bool correctBusType = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals(_mainWindow.SelectedDepot, StringComparison.OrdinalIgnoreCase))
                {
                    inCorrectDepot = true;
                    continue;
                }
                if (inCorrectDepot && line.Equals("[aigroup_depot_typgroup_2]", StringComparison.OrdinalIgnoreCase))
                {
                    inAigroupType = true;
                    continue;
                }
                if (line.IndexOf(_mainWindow.Pbustype, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    correctBusType = true;
                }
                if (inAigroupType && inCorrectDepot)
                {
                    if (line.IndexOf("[end]", StringComparison.OrdinalIgnoreCase) >= 0 && inCorrectDepot == true)
                    {
                        correctBusType = false;
                        inAigroupType = false;
                        break;
                    }
                    if (lines[i].IndexOf(_mainWindow.Pbustype, StringComparison.OrdinalIgnoreCase) >= 0 && inCorrectDepot == true)
                    {
                        lines[i] = _mainWindow.NewBusFile;
                        break;
                    }
                }
            }
            System.IO.File.WriteAllLines(_mainWindow.Pfile, lines);
            _mainWindow.CheckAndReadNosFile(_mainWindow.BusFileFullPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }


    }
    public void DeleteLondonReg()
    {

        var lines = System.IO.File.ReadAllLines(_mainWindow.Pfile).ToList();
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

            if (line.Equals(_mainWindow.SelectedDepot, StringComparison.OrdinalIgnoreCase)) // Match the depot name
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
                foreach (var reg in _mainWindow.RegInfo)
                {
                    if (!string.IsNullOrEmpty(reg) && line.Contains(reg))
                    {
                        lines[i] = line.Replace(reg, "");
                        break; // Only replace the first occurrence
                    }

                }

            }
        }
        System.IO.File.WriteAllLines(_mainWindow.Pfile, lines);

    }
}