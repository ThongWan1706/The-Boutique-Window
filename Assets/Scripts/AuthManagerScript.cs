using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class AuthManagerScript : MonoBehaviour
{
    public TMP_InputField email; //For the input fields in the canvas
    public TMP_InputField password; //For the input fields in the canvas
    public TMP_Text errormessage; //For the error message
    public GameObject errorPopupPanel; //For the ErrorPopupPanel
    public GameObject resetPasswordPopupPanel; //For the resetPasswordPanel
    public TMP_Text resetPasswordmessage; //For the reset password message

    //In default the error panel is hidden
    public void CloseErrorPopup()
    {
        if (errorPopupPanel != null)
            errorPopupPanel.SetActive(false);
    }

    //In default the reset panel is hidden
    public void CloseResetPasswordPopup()
    {
        if (resetPasswordPopupPanel != null)
            resetPasswordPopupPanel.SetActive(false);
    }

    //Validations
    private bool CheckInputs()
    {
        errormessage.text = "";
        //Checking for errors for the inputfields
        if (email.text.Length == 0)
        {
            errormessage.text = "Please enter a proper email address.";
            if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            return false;
        }
        else if (password.text.Length == 0)
        {
            errormessage.text = "Please enter a proper password.";
            if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            return false;
        }
        else if (password.text.Length < 8)
        {
            errormessage.text = "Please enter at least 8 characters password";
            if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            return false;
        }
        return true;
    }

    //Check whether the player is at the proper page
    public void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += AuthStateChangedHandler;
    }

    public void AuthStateChangedHandler(object sender, EventArgs args)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log($"Current user is {FirebaseAuth.DefaultInstance.CurrentUser.Email}");
        }
        else
        {
            Debug.Log("No user are logged in at the moment");
        }
    }

    //For signing up a new account [Create account]
    public void Signup()
    {
        SceneManager.LoadScene("SignUp");
    }

    //For login [Allow the user to enter into the playscene only when they already have an account]
    public void Login()
    {
        if (CheckInputs() == false) return;

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email.text, password.text).ContinueWithOnMainThread(task =>
        {
            //if have error/missing fields
            if (task.IsFaulted)
            {
                Debug.Log(task.Exception);

                var firebaseException = task.Exception.GetBaseException() as FirebaseException;
                var errorCode = (AuthError)firebaseException.ErrorCode;

                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        errormessage.text = "Please enter an email address";
                        break;
                    case AuthError.MissingPassword:
                        errormessage.text = "Please enter a password";
                        break;
                    case AuthError.WrongPassword:
                        errormessage.text = "Please enter the correct password";
                        break;
                    case AuthError.InvalidEmail:
                        errormessage.text = "Please enter a proper email";
                        break;
                    case AuthError.UserNotFound:
                        errormessage.text = "Account does not exist";
                        break;
                    default:
                        errormessage.text = "Account does not exist or invalid credentials";
                        break;
                }
                
                // Shows up the error panel
                if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            }
            else if (task.IsCanceled)
            {
                Debug.Log("Login is cancelled, please try again");
                errormessage.text = "Login is cancelled, please try again";
                if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            }
            else
            {
                var AuthResult = task.Result;
                Debug.Log($"User logged in, user ID is: {AuthResult.User.UserId}");

                FirebaseDatabase.DefaultInstance.GetReference("players").Child(AuthResult.User.UserId).GetValueAsync().ContinueWithOnMainThread(dbTask =>
                {
                    if (dbTask.IsFaulted)
                    {
                        Debug.Log(dbTask.Exception);
                    }
                    else if (dbTask.IsCanceled)
                    {
                        Debug.Log("Database read cancelled");
                    }
                    else
                    {
                        SceneManager.LoadScene("GameScene");
                    }
                });
            }
        });
    }

    //For Forget Password
    public void ForgetPassword()
    {
        //If email is empty, use the default error popup and message
        if (string.IsNullOrEmpty(email.text))
        {
            errormessage.text = "Please enter a proper email address.";
            if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            return;
        }

        FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email.text).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log(task.Exception);
                
                // Display error inside the error popup or reset password message
                errormessage.text = "Failed to send password reset email. Please check if the email is correct.";
                if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            }
            else if (task.IsCanceled)
            {
                errormessage.text = "Request cancelled.";
                if (errorPopupPanel != null) errorPopupPanel.SetActive(true);
            }
            else
            {
                // Success: Update the reset password panel message and open it
                if (resetPasswordmessage != null)
                {
                    resetPasswordmessage.text = "Please check your email for steps to reset the password";
                }
                if (resetPasswordPopupPanel != null) 
                {
                    resetPasswordPopupPanel.SetActive(true);
                }
            }
        });
    }

    //For Signout
    public void SignOut()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("LoginScene");
    }
}