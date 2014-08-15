﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;

namespace MegaApp.Models
{
    class LoginViewModel : BaseViewModel, MRequestListenerInterface
    {
        private readonly MegaSDK _megaSdk;

        public LoginViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
            this.LoginCommand = new DelegateCommand(this.DoLogin);
            this.NavigateCreateAccountCommand = new DelegateCommand(NavigateCreateAccount);
        }

        #region Methods

        private void DoLogin(object obj)
        {
            if (CheckInputParameters())
            {
                this._megaSdk.login(Email, Password, this);
            }
            else
            {
                MessageBox.Show(AppMessages.RequiredFieldsLogin, AppMessages.RequiredFields_Title,
                        MessageBoxButton.OK);
            }
        }
        private static void NavigateCreateAccount(object obj)
        {
            NavigateService.NavigateTo(typeof(CreateAccountPage), NavigationParameter.Normal);
        }

        private bool CheckInputParameters()
        {
            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password);
        }

        private static void SaveLoginData(string email, string session)
        {
            SettingsService.SaveMegaLoginData(email, session);
        }
        
        #endregion

        #region Commands

        public ICommand LoginCommand { get; set; }

        public ICommand NavigateCreateAccountCommand { get; set; }

        #endregion

        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        #endregion

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);

                this.ControlState = true;

                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    if (RememberMe)
                        SaveLoginData(Email, api.dumpSession());

                    NavigateService.NavigateTo(typeof(MainPage),NavigationParameter.Login);
                }
                else
                    MessageBox.Show(String.Format(AppMessages.LoginFailed, e.getErrorString()),
                        AppMessages.LoginFailed_Title, MessageBoxButton.OK);
            });
        }
       
        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgessService.SetProgressIndicator(true, AppMessages.ProgressIndicator_Login);
            });
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
                MessageBox.Show(String.Format(AppMessages.LoginFailed, e.getErrorString()),
                    AppMessages.LoginFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion
    }
}
