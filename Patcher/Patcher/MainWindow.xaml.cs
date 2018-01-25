﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using IWshRuntimeLibrary;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using Popup = System.Windows.MessageBox;

namespace Patcher
{
    public partial class MainWindow : Window
    {
        public static BackgroundWorker patcherWorker = new BackgroundWorker(); //replace threading

        public volatile Boolean debug = false; //go through the motions without copying all of the files
        public volatile string gameExec = ""; //init gameExec
        public volatile string gameDir = ""; //init gameDir
        public volatile string cooppatchFile = ""; //init cooppatchFile
        public volatile string path = @"C:\\"; //init default path
        public volatile string consoleKey; //init -- set in button_Click
        public volatile string fileCopying = "files..."; //current file copying
        public volatile int gameID; //init game id
        public volatile ArrayList mods = new ArrayList();


        public MainWindow()
        {
            InitializeComponent();

            progressBar.Maximum = 100;
            progressBar.Value = 0;

            patcherWorker.DoWork += new DoWorkEventHandler(patcherWorker_DoWork);
            patcherWorker.RunWorkerCompleted += patcherWorker_RunWorkerCompleted;
            patcherWorker.ProgressChanged += patcherWorker_ProgressChanged;
            patcherWorker.WorkerReportsProgress = true;
            patcherWorker.WorkerSupportsCancellation = true;
        }

        public void button_Click(object sender, RoutedEventArgs e) // patch borderlands2
        {
            gameID = (comboBoxGame.SelectedIndex + 2); //calculate id of game from index of selected dropdown item

            buttonPatch.IsEnabled = false; //disable patch button
            buttonPatch.Visibility = Visibility.Hidden; //hide the patch button
            comboBoxGame.Visibility = Visibility.Hidden;
            comboBoxConsoleKey.Visibility = Visibility.Hidden;
            labelConsoleKey.Visibility = Visibility.Hidden;
            checkBoxCommunityPatch.Visibility = Visibility.Hidden;
            Height = 115; //shorten window
            taskbarInfo.ProgressState = TaskbarItemProgressState.Normal;
            taskbarInfo.ProgressValue = 0; //reset progress to 0
            progressBar.Visibility = Visibility.Visible; //make visible
            labelProgressText.Visibility = Visibility.Visible; //make visible

            consoleKey = comboBoxConsoleKey.Text; //console key
            mods.Add(cooppatchFile);
            if (checkBoxCommunityPatch.IsChecked == true && gameID == 2) //Community patch - only with Borderlands 2
            {
                mods.Add("Patch.txt");
            }


            switch (gameID) //depending on game, set variables accordingly
            {
                case 3:
                    gameExec = "BorderlandsPreSequel.exe";
                    gameDir = "BorderlandsPreSequel";
                    cooppatchFile = "cooppatch.txt";
                    break;
                default: //2 or incase some how there isnt a variable
                    gameExec = "Borderlands2.exe";
                    gameDir = "Borderlands 2";
                    cooppatchFile = "cooppatch.txt";
                    break;
            }

            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "Borderlands|*.exe";
            fileDialog.Title = "Open " + gameExec;
            fileDialog.InitialDirectory = @"C:\\Program Files (x86)\\Steam\\SteamApps\\common\\" + gameDir + "\\Binaries\\Win32"; //I guess this isnt working
            fileDialog.RestoreDirectory = true; //this either
            var result = fileDialog.ShowDialog(); //open file picker dialog

            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    path = fileDialog.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }

            patcherWorker.RunWorkerAsync(); //run the patch function
        }

        private void patcherWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage; //loading bar
            taskbarInfo.ProgressValue = e.ProgressPercentage; //taskbar

            switch(e.ProgressPercentage)
            {
                case 10:
                    labelProgressText.Content = "Copying " + fileCopying; //current file copying
                    break;
                case 40:
                    labelProgressText.Content = "Downloading patches";
                    break;
                case 50:
                    labelProgressText.Content = "Hacking your Minecraft account";
                    break;
                case 60:
                    labelProgressText.Content = "Decompressing some stuff";
                    break;
                case 70:
                    labelProgressText.Content = "Making a sandwich";
                    break;
                case 75:
                    labelProgressText.Content = "Installing viruses";
                    break;
                case 80:
                    labelProgressText.Content = "Recombobulation the flux capacitor";
                    break;
                case 90:
                    labelProgressText.Content = "Climaxing";
                    break;
                case 100:
                    labelProgressText.Content = "All done";
                    Popup.Show("Done! A Shortcut was placed on your desktop. Press '~' in game to open up console.");
                    break;
                default:
                    break;
            }
        }

        private void patcherWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Height = 165; //resize back to original size
            buttonPatch.IsEnabled = true; //disable patch button
            buttonPatch.Visibility = Visibility.Visible; //hide the patch button
            comboBoxGame.Visibility = Visibility.Visible;
            comboBoxConsoleKey.Visibility = Visibility.Visible;
            labelConsoleKey.Visibility = Visibility.Visible;
            checkBoxCommunityPatch.Visibility = Visibility.Visible;
            //progressBarStatic.IsIndeterminate = false; //disable MARQUEE style
            progressBar.Visibility = Visibility.Hidden; //make the loading bar invisible
            labelProgressText.Visibility = Visibility.Hidden; //make invisible
            taskbarInfo.ProgressState = TaskbarItemProgressState.None; //hide the loading bar in the taskbar
        }

        //private void pat

        public void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) //This function is taken straight from stackoverflow thanks to Konrad Rudolph. Rewrite
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                if (source.FullName != dir.FullName && source.Name != "server") //prevent infinite copy loop
                {
                    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                }
            foreach (FileInfo file in source.GetFiles())
                if (file.Name != "dbghelp.dll") //dbhhelp was causing issued for god knows why
                {
                    try
                    {
                        file.CopyTo(System.IO.Path.Combine(target.FullName, file.Name));
                        fileCopying = file.Name; //update the current copying file with the name of the file being copied
                        patcherWorker.ReportProgress(10);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("ERROR: Could not copy file " + file.Name);
                    }
                }
        }

        private void patcherWorker_DoWork(object sender, DoWorkEventArgs e) //the main function
        {
            DirectoryInfo iBL = new DirectoryInfo(path); //bl = path to Borderlands exe
            DirectoryInfo inputDir = new DirectoryInfo(iBL + @"..\\..\\..\\..\\"); //convert to directory - IDK why I need more ..\\s then I actually should but it works so who cares
            DirectoryInfo outputDir = new DirectoryInfo(inputDir + @"\\server"); //convert to directory
            DirectoryInfo oBL = new DirectoryInfo(outputDir + @"\\Binaries\\Win32\\" + gameExec);
            DirectoryInfo iWillowGameUPK = new DirectoryInfo(inputDir + @"\\WillowGame\\CookedPCConsole\\WillowGame.upk"); // path to WillowGame.upk
            DirectoryInfo oWillowGameUPK = new DirectoryInfo(outputDir + @"\\WillowGame\\CookedPCConsole\\WillowGame.upk"); // path to WillowGame.upk
            DirectoryInfo iEngineUPK = new DirectoryInfo(inputDir + @"\\WillowGame\\CookedPCConsole\\Engine.upk"); // path to Engine.upk
            DirectoryInfo oEngineUPK = new DirectoryInfo(outputDir + @"\\WillowGame\\CookedPCConsole\\Engine.upk"); // path to Engine.upk

            String decompress = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "decompress.exe");
            Boolean skipCopy = false;

            patcherWorker.ReportProgress(10); //set loading to 10%

            if (System.IO.File.Exists(iBL.FullName)) //if borderlands exec exists
            {
                // -- COPY INPUT TO OUTPUT --
                try
                {
                    if (outputDir.Exists) //if the server folder exists
                    {
                        System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("It looks like a patched version of Borderlands already exists, would you like to replace it? Clicking 'No' will skip the copy and attempt to patch the existing files, and 'Cancel' will stop the patcher altogether.", "ERROR: Output folder already exists!", System.Windows.Forms.MessageBoxButtons.YesNoCancel);

                        switch (dialogResult)
                        {
                            case System.Windows.Forms.DialogResult.Yes:
                                if (!debug && !skipCopy) //if not in debug mode
                                {
                                    outputDir.Delete(true); //delete the server folder recursively
                                }
                                skipCopy = false ;//continue
                                break;
                            case System.Windows.Forms.DialogResult.No:
                                skipCopy = true;//skip the copy
                                break;
                            case System.Windows.Forms.DialogResult.Cancel:
                                patcherWorker.CancelAsync(); //cancel
                                patcherWorker.Dispose(); 
                                //Close(); //terminate thread
                                break;
                        }
                    }

                    if (!debug && !skipCopy) //if not in debug mode
                    {
                        CopyFilesRecursively(inputDir, outputDir); //backup borderlands to server subdir
                    }
                }
                catch (IOException)
                {
                    Popup.Show("ERROR: Cannot copy Borderlands. Does it already exist?");
                }

                patcherWorker.ReportProgress(40); //set loadingprogress to 40%
                // -- COPY PATCHES TO BINARIES -- 
                using (WebClient myWebClient = new WebClient()) //download file
                {
                    try
                    {
                        myWebClient.DownloadFile("https://raw.githubusercontent.com/RobethX/BL2-MP-Mods/master/CoopPatch/" + cooppatchFile, outputDir.FullName + @"\Binaries\" + cooppatchFile);
                    }
                    catch (WebException)
                    {
                        //log
                    }

                    //Community patch
                    if (mods.Contains("Patch.txt"))
                    {
                        try
                        {
                            myWebClient.DownloadFile("https://www.dropbox.com/s/kxvf8w3ul4zuh93/Patch.txt", outputDir.FullName + @"\Binaries\Patch.txt");
                        }
                        catch (WebException)
                        {
                            //log
                        }
                    }
                }

                patcherWorker.ReportProgress(50); //set loadingprogress to 50%
                // -- RENAME UPK AND DECOMPRESSEDSIZE --
                try //incase it's already moved
                {
                    System.IO.File.Move(oWillowGameUPK.FullName + ".uncompressed_size", oWillowGameUPK.FullName + ".uncompressed_size.bak"); //backup WillowGame.upk.uncompressed_size
                    System.IO.File.Copy(oWillowGameUPK.FullName, oWillowGameUPK.FullName + ".bak"); //backup upk
                    System.IO.File.Move(oEngineUPK.FullName + ".uncompressed_size", oEngineUPK.FullName + ".uncompressed_size.bak"); //backup Engine.upk.uncompressed_size
                    System.IO.File.Copy(oEngineUPK.FullName, oEngineUPK.FullName + ".bak"); //backup upk
                }
                catch (IOException)
                {
                    //log
                }

                patcherWorker.ReportProgress(60); //set loadingprogress to 60%
                // -- DECOMPRESS UPK --
                //var decompressing = System.Diagnostics.Process.Start(decompress, "-game=border -out=" + outputDir + @"\\WillowGame\\CookedPCConsole\\ " + iUPK.FullName); //decompress WillowGame.UPK
                var decompressingWillowGame = System.Diagnostics.Process.Start(decompress, "-game=border -log=decompress.log " + '"' + iWillowGameUPK.FullName + '"'); //decompress WillowGame.UPK
                decompressingWillowGame.WaitForExit(); //wait for decompress.exe to finish
                var decompressingEngine = System.Diagnostics.Process.Start(decompress, "-game=border -log=decompress.log " + '"' + iEngineUPK.FullName + '"'); //decompress Engine.UPK
                decompressingEngine.WaitForExit(); //wait for decompress.exe to finish
                FileInfo decompressedWillowGameUPK = new FileInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), @"unpacked\\", iWillowGameUPK.Name));
                FileInfo decompressedEngineUPK = new FileInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), @"unpacked\\", iEngineUPK.Name));

                try
                {
                    decompressedWillowGameUPK.CopyTo(oWillowGameUPK.FullName, true); //move upk to cookedpcconsole
                    decompressedEngineUPK.CopyTo(oEngineUPK.FullName, true); //move upk to cookedpcconsole

                }
                catch (IOException)
                {
                    Popup.Show("ERROR: Could not find decompressed UPK"); //for debugging
                }

                patcherWorker.ReportProgress(70); //set loadingprogress to 70%
                // -- HEX EDITING --
                switch (gameID)
                {
                    case 3: //tps
                        try
                        {
                            var streamWillowGameUPKTPS = new FileStream(oWillowGameUPK.FullName, FileMode.Open, FileAccess.ReadWrite);

                            // -- DEVELOPER MODE --
                            streamWillowGameUPKTPS.Position = 0x0079ACE7;
                            streamWillowGameUPKTPS.WriteByte(0x27);

                            // -- EVERY PLAYER GETS THEIR OWN TEAM --
                            streamWillowGameUPKTPS.Position = 0x0099D50F;
                            streamWillowGameUPKTPS.WriteByte(0x04);
                            streamWillowGameUPKTPS.Position = 0x0099D510;
                            streamWillowGameUPKTPS.WriteByte(0x00);
                            streamWillowGameUPKTPS.Position = 0x0099D511;
                            streamWillowGameUPKTPS.WriteByte(0x82);
                            streamWillowGameUPKTPS.Position = 0x0099D512;
                            streamWillowGameUPKTPS.WriteByte(0xB1);
                            streamWillowGameUPKTPS.Position = 0x0099D513;
                            streamWillowGameUPKTPS.WriteByte(0x00);
                            streamWillowGameUPKTPS.Position = 0x0099D514;
                            streamWillowGameUPKTPS.WriteByte(0x00);
                            streamWillowGameUPKTPS.Position = 0x0099D515;
                            streamWillowGameUPKTPS.WriteByte(0x06);
                            streamWillowGameUPKTPS.Position = 0x0099D516;
                            streamWillowGameUPKTPS.WriteByte(0x44);
                            streamWillowGameUPKTPS.Position = 0x0099D517;
                            streamWillowGameUPKTPS.WriteByte(0x00);
                            streamWillowGameUPKTPS.Position = 0x0099D518;
                            streamWillowGameUPKTPS.WriteByte(0x04);
                            streamWillowGameUPKTPS.Position = 0x0099D519;
                            streamWillowGameUPKTPS.WriteByte(0x24);
                            streamWillowGameUPKTPS.Position = 0x0099D51A;
                            streamWillowGameUPKTPS.WriteByte(0x00);

                            streamWillowGameUPKTPS.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify upk files");
                        }

                        patcherWorker.ReportProgress(75); //set loadingprogress to 75%

                        // -- HEX EDIT ENGINE.UPK --
                        try
                        {
                            var streamEngineTPS = new FileStream(oBL.FullName, FileMode.Open, FileAccess.ReadWrite);
                            // -- DON'T UPDATE PLAYERCOUNT --

                            //streamEngineTPS.Position = 0x003F69A4;
                            //streamEngineTPS.WriteByte(0x1E);
                            streamEngineTPS.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify upk files");
                        }

                        patcherWorker.ReportProgress(80); //set loadingprogress to 80%
                        // -- HEX EDIT BORDERLANDSPRESEQUEL.EXE --
                        try
                        {
                            var streamBLTPS = new FileStream(oBL.FullName, FileMode.Open, FileAccess.ReadWrite);
                            streamBLTPS.Position = 0x00D8BD1F;
                            streamBLTPS.WriteByte(0xFF);
                            for (long i = 0x018E9C33; i <= 0x018E9C39; i++)
                            {
                                streamBLTPS.Position = i;
                                streamBLTPS.WriteByte(0x00);
                            }
                            streamBLTPS.Position = 0x01D3D699; //find willowgame.upk
                            streamBLTPS.WriteByte(0x78); //willowgame.upk > xillowgame.upk
                            streamBLTPS.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify executable");
                        }
                        break;
                    case 2: //bl2
                        // -- HEX EDIT WILLOWGAME --
                        try
                        {
                            var streamWillowGame2 = new FileStream(oWillowGameUPK.FullName, FileMode.Open, FileAccess.ReadWrite);

                            // -- DEVELOPER MODE --
                            streamWillowGame2.Position = 0x006924C7;
                            streamWillowGame2.WriteByte(0x27);

                            // -- EVERY PLAYER GETS THEIR OWN TEAM --
                            streamWillowGame2.Position = 0x007F9151;
                            streamWillowGame2.WriteByte(0x04);
                            streamWillowGame2.Position = 0x007F9152;
                            streamWillowGame2.WriteByte(0x00);
                            streamWillowGame2.Position = 0x007F9153;
                            streamWillowGame2.WriteByte(0xC6);
                            streamWillowGame2.Position = 0x007F9154;
                            streamWillowGame2.WriteByte(0x8B);
                            streamWillowGame2.Position = 0x007F9155;
                            streamWillowGame2.WriteByte(0x00);
                            streamWillowGame2.Position = 0x007F9156;
                            streamWillowGame2.WriteByte(0x00);
                            streamWillowGame2.Position = 0x007F9157;
                            streamWillowGame2.WriteByte(0x06);
                            streamWillowGame2.Position = 0x007F9158;
                            streamWillowGame2.WriteByte(0x44);
                            streamWillowGame2.Position = 0x007F9159;
                            streamWillowGame2.WriteByte(0x00);
                            streamWillowGame2.Position = 0x007F915A;
                            streamWillowGame2.WriteByte(0x04);
                            streamWillowGame2.Position = 0x007F915B;
                            streamWillowGame2.WriteByte(0x24);
                            streamWillowGame2.Position = 0x007F915C;
                            streamWillowGame2.WriteByte(0x00);
                            streamWillowGame2.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify upk files");
                        }

                        patcherWorker.ReportProgress(75); //set loadingprogress to 75%

                        // -- HEX EDIT ENGINE.UPK --
                        try
                        {
                            var streamEngine2 = new FileStream(oBL.FullName, FileMode.Open, FileAccess.ReadWrite);

                            // -- EffectiveNumPlayers --> NumSpectators --
                            /*
                            streamEngine2.Position = 0x003F699F;
                            streamEngine2.WriteByte(0x22);
                            streamEngine2.Position = 0x003F7085;
                            streamEngine2.WriteByte(0x22);
                            */
                            streamEngine2.Position = 0x003FC015;
                            streamEngine2.WriteByte(0x22);

                            // -- NumPlayers --> EffectiveNumPlayers --
                            /*
                            streamEngine2.Position = 0x003F69A4;
                            streamEngine2.WriteByte(0x1E);
                            streamEngine2.Position = 0x003F708A;
                            streamEngine2.WriteByte(0x1E);
                            */
                            streamEngine2.Position = 0x003FC01A;
                            streamEngine2.WriteByte(0x1E);
                           
                            streamEngine2.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify upk files");
                        }

                        patcherWorker.ReportProgress(80); //set loadingprogress to 80%

                        // -- HEX EDIT BORDERLANDS2.EXE --
                        try
                        {
                            var streamBL2 = new FileStream(oBL.FullName, FileMode.Open, FileAccess.ReadWrite);
                            streamBL2.Position = 0x004F2590;
                            streamBL2.WriteByte(0xFF);
                            for (long i = 0x01B94B0C; i <= 0x01B94B10; i++)
                            {
                                streamBL2.Position = i;
                                streamBL2.WriteByte(0x00);
                            }
                            streamBL2.Position = 0x01EF17F9; //find upk
                            streamBL2.WriteByte(0x78); //willowgame.upk > xillowgame.upk
                            streamBL2.Close();
                        }
                        catch (IOException)
                        {
                            Popup.Show("ERROR: Could not modify executable");
                        }
                        break;
                }

                patcherWorker.ReportProgress(90); //set loadingprogress to 90%
                // -- CREATE SHORTCUT --
                try
                {
                    String execMods = "";
                    foreach(String mod in mods)
                    {
                        execMods = execMods + " -exec=" + mod;
                    }

                    WshShell shell = new WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\\" + gameDir + " - Robeth's Unlimited COOP Mod.lnk") as IWshShortcut;
                    shortcut.Arguments = "-log -debug -codermode -nosplash" + execMods;
                    shortcut.TargetPath = oBL.FullName;
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "Robeth's Borderlands COOP patch";
                    shortcut.WorkingDirectory = (Directory.GetParent(oBL.FullName)).FullName;
                    shortcut.IconLocation = (oBL + ",1");
                    shortcut.Save();
                }
                catch (IOException)
                {
                    Popup.Show("ERROR: Failed to create shortcut");
                }

                // -- ENABLE CONSOLE -- RIPPED STRAIGHT FROM BUGWORM's BORDERLANDS2PATCHER!!!!!
                try
                {
                    int i; //for temp[i]
                    string tmpPath = @"\\my games\\" + gameDir + "\\willowgame\\Config\\WillowInput.ini";
                    string iniPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + tmpPath;
                    string[] iniLine = System.IO.File.ReadAllLines(iniPath);
                    for (i = 0; i < iniLine.Length; i++)
                    {
                        if (iniLine[i].StartsWith("ConsoleKey="))
                            break;
                    }
                    iniLine[i] = "ConsoleKey=" + consoleKey;
                    System.IO.File.WriteAllLines(iniPath, iniLine);
                }
                catch (IOException)
                {
                    Popup.Show("ERROR: Failed to enable console");
                }
                //END OF BUGWORM'S CODE

                // -- DONE --
                patcherWorker.ReportProgress(100); //set loadingprogress to 100%
            }
            else
            {
                Popup.Show("ERROR: " + gameExec + " not found.");
            }
        }
    }
}
