using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using TMPro; // Import for TextMeshPro support

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public TMP_Text loginMessageText;
    public GameObject loginPanel;  // Assign in Inspector

    [Space]
    [Header("Forgot Password")]
    public InputField resetEmailField;
    public TMP_Text resetMessageText;
    public GameObject forgotPasswordPanel;  // Assign in Inspector

    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;
    public TMP_Text registerMessageText;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        loginMessageText.text = "Logging in...";
        StartCoroutine(HideMessageAfterDelay(loginMessageText));

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed: ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Invalid email address.";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Incorrect password.";
                    break;
                case AuthError.UserNotFound:
                    failedMessage = "No account found. Please register first.";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is required.";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is required.";
                    break;
                default:
                    failedMessage = "Login Failed. Please try again.";
                    break;
            }

            loginMessageText.text = failedMessage;
        }
        else
        {
            user = loginTask.Result.User;

            if (user.IsEmailVerified)
            {
                loginMessageText.text = "Login successful!";
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
            else
            {
                loginMessageText.text = "Please verify your email before logging in.";
                user.SendEmailVerificationAsync();
            }
        }

        StartCoroutine(HideMessageAfterDelay(loginMessageText));
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            registerMessageText.text = "Username cannot be empty.";
        }
        else if (string.IsNullOrEmpty(email))
        {
            registerMessageText.text = "Email cannot be empty.";
        }
        else if (password != confirmPassword)
        {
            registerMessageText.text = "Passwords do not match.";
        }
        else
        {
            registerMessageText.text = "Registering...";
            StartCoroutine(HideMessageAfterDelay(registerMessageText));

            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed: ";

                switch (authError)
                {
                    case AuthError.EmailAlreadyInUse:
                        failedMessage = "Account already exists. Please log in.";
                        break;
                    case AuthError.InvalidEmail:
                        failedMessage += "Invalid email address.";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is required.";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is required.";
                        break;
                    default:
                        failedMessage = "Registration Failed. Please try again.";
                        break;
                }

                registerMessageText.text = failedMessage;
            }
            else
            {
                user = registerTask.Result.User;
                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    user.DeleteAsync();
                    registerMessageText.text = "Profile update failed. Try again.";
                }
                else
                {
                    registerMessageText.text = "Registration successful! Please check your email to verify your account.";
                    var emailVerificationTask = user.SendEmailVerificationAsync();
                    yield return new WaitUntil(() => emailVerificationTask.IsCompleted);

                    if (emailVerificationTask.Exception != null)
                    {
                        registerMessageText.text = "Could not send verification email. Try again.";
                    }
                    else
                    {
                        UIManager.Instance.OpenLoginPanel();
                    }
                }
            }
        }

        StartCoroutine(HideMessageAfterDelay(registerMessageText));
    }

    public void ForgotPassword()
    {
        StartCoroutine(ResetPasswordAsync(resetEmailField.text));
    }

    private IEnumerator ResetPasswordAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            resetMessageText.text = "Please enter your email.";
        }
        else
        {
            var passwordResetTask = auth.SendPasswordResetEmailAsync(email);
            yield return new WaitUntil(() => passwordResetTask.IsCompleted);

            if (passwordResetTask.Exception != null)
            {
                FirebaseException firebaseException = passwordResetTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                if (authError == AuthError.UserNotFound)
                {
                    resetMessageText.text = "No account found with this email.";
                }
                else
                {
                    resetMessageText.text = "Error sending password reset email.";
                }
            }
            else
            {
                resetMessageText.text = "Password reset email sent! Check your inbox.";
            }
        }

        StartCoroutine(HideMessageAfterDelay(resetMessageText));
    }

    public void OpenForgotPasswordPanel()
    {
        loginPanel.SetActive(false);
        forgotPasswordPanel.SetActive(true);
    }

    public void CloseForgotPasswordPanel()
    {
        forgotPasswordPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    private IEnumerator HideMessageAfterDelay(TMP_Text messageText)
    {
        yield return new WaitForSeconds(3f);
        messageText.text = "";
    }
}
