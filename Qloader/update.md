﻿# Bot

## WindowViewModel.StartUp
Убрать всё от начала до

    this.SettingsControl()

Убрать всё после

    this.SettingsControl()
до

    MainHelper.ShowPage(new MainPage());

## или WindowViewModel.\<StartUp>d__45
Убрать всё от

    awaiter = Authentication.Check(windowViewModel.Usb).GetAwaiter();
до

    windowViewModel.SettingsControl();

Убрать всё после

    windowViewModel.SettingsControl();
до

    MainHelper.ShowPage(new MainPage());

## WindowViewModel.Close
Убрать

    await account.LogOut(AccountType.Normal);

## или WindowViewModel.\<Close>d__37
Убрать от

    awaiter = this.<account>5__3.LogOut(AccountType.Normal).GetAwaiter();

до

    this.<account>5__3 = null;

## App.Main
Убрать 

    new SplashScreen("resources/startlogo.png").Show(true);

## Автоматизация загрузки аккаунтов MainViewModel.LoadAccountsFromFile
Убрать всё, относящееся к dialog. 

Добавить строку

    string FileName = "qBotAccounts.xml";

Заменить
    dialog.FileName

на

    FileName
    
## Client.Client
После new HttpClient добавить строку
System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
(в обоих конструкторах)

# Flash

## WindowViewModel.StartUp
Убрать от

    GlobalSettings.GetInstance();

до

    this.SettingsControl();

Убрать от

    this.SettingsControl();

до

    MainHelper.ShowPage(new LoginPage());

## или WindowViewModel.\<StartUp>d__38
Убрать от

    awaiter = Authentication.CheckFlash(windowViewModel.Usb).GetAwaiter();
до

    windowViewModel.SettingsControl();

Убрать всё после

    windowViewModel.SettingsControl();

до

    MainHelper.ShowPage(new LoginPage());

## WindowViewModel.Close

Убрать

    await account.LogOut(AccountType.Normal);

## или WindowViewModel.\<Close>d__37
Убрать от
    
    awaiter = this.\<acc>5__2.LogOut(AccountType.Normal).GetAwaiter();

до

    this.window.Close();

## App.Main
Убрать
    
    new SplashScreen("resources/flash400x400.png").Show(true);

## Client.Client
После new HttpClient добавить строку
System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
(в обоих конструкторах)

# Holder

## MainWindowViewModel.OnLoad
Убрать всё
