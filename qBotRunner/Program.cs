﻿using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Automation; 

namespace qBotRunner 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            if (args.Length == 2)
            {
                RunBot(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("Usage: qBotRunner <bot_executable_path> <accounts_file_path>");
            }
        }
        // ----------------------------------------------------------------
        private static void RunBot(string exeFile, string accountsFile)
        {
            try 
            { 
                Process bot = Process.Start(exeFile);
                
                AutomationElement mainWindow = GetMainWindow(bot);

                AutomationElement rootPane = mainWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
                if (null == rootPane) throw new Exception("Root pane not found");
                
                AutomationElement loadButton = rootPane.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Load accounts list"));
                if (null == loadButton) throw new Exception("Load accounts list button not found");

                AutomationElement startButton = rootPane.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Start all"));
                if (null == startButton) throw new Exception("Start all button not found");

                Console.WriteLine("Click \"Load accounts list\" button");
                ((InvokePattern)loadButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                
                Thread.Sleep(1000);
                
                ApplyInputFile(mainWindow, accountsFile);

                bool stillWaiting = true;
                do
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting for the accounts loading");
                    stillWaiting = !(Boolean)startButton.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty);
                }
                while(stillWaiting);
                
                Console.WriteLine("Click \"Start all\" button");
                ((InvokePattern)startButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine("Fatal: " + ex.Message); 
            } 
        }
        // ----------------------------------------------------------------
        private static AutomationElement GetMainWindow(Process bot) 
        {
            AutomationElement desktop = AutomationElement.RootElement; 
            
            for (int i = 0; i < 5; ++i)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Waiting for the main window");
                
                AutomationElement mainWindow = desktop.FindFirst(TreeScope.Children, new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "MainWindow")
                    , new PropertyCondition(AutomationElement.ProcessIdProperty, bot.Id)
                )); 
                if (null != mainWindow)
                {
                    return mainWindow;
                }
            }
            
            throw new Exception("Main window not found");
        }
        // ----------------------------------------------------------------
        private static void ApplyInputFile (AutomationElement mainWindow, string filename)
        {
            AutomationElement openDialog = mainWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Open"));
            if (null == openDialog) throw new Exception("Open dialog not found");

            AutomationElement fileNameComboBox = openDialog.FindFirst(TreeScope.Children
                , new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "File name:")
                    , new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox)
                )
            );
            if (null == fileNameComboBox) throw new Exception("File name combobox not found");
            
            // Set file name
            ((ValuePattern)fileNameComboBox.GetCurrentPattern(ValuePattern.Pattern)).SetValue(filename);

            Thread.Sleep(500);
            AutomationElement openButton = openDialog.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Open"));
            if (null == openButton) throw new Exception("Open button not found");
            
            // Click Open button
            ((InvokePattern)openButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
        }
        // ----------------------------------------------------------------
        private static void PrintChildren (AutomationElement element)
        {
            Console.WriteLine($"Children of {element.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString()}:");
            foreach (AutomationElement child in element.FindAll(TreeScope.Children, Condition.TrueCondition)) 
            {
                string? name = child.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString();
                string type = ((ControlType)child.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty)).ProgrammaticName;
                Console.WriteLine($"type = \"{type}\", name = \"{name}\"");
            }
        }
    }
}