using Terminal.Gui;
using DxtrPasswordGen;

class Dexter
{
    #pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'int int.Parse(string s)'.

    private static Window? mainWindow;
    private static string? currentPassword;

    private protected static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Application.Init();
        Colors.Base = new ColorScheme();
        ShowMainMenu();
        Application.Run();
        Application.Shutdown();
    }

    private static void ShowMainMenu()
    {
        Application.Top.RemoveAll();

        mainWindow = new Window("Dexter v1.2 - What can Dexter do for you?")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var header = new Label(GetHeaderText())
        {
            X = Pos.Center(),
            Y = 1,
            Width = Dim.Fill(),
            Height = 10,
            TextAlignment = TextAlignment.Centered
        };

        var btnPasswordGen = new Button("1. Secure Password Generation")
        {
            X = Pos.Center(),
            Y = 12,
            Width = 30
        };

        var btnEncryption = new Button("2. File Encryption/Decryption")
        {
            X = Pos.Center(),
            Y = 14,
            Width = 30
        };

        var btnHealthCheck = new Button("3. General File Health Checks(Zips)")
        {
            X = Pos.Center(),
            Y = 16,
            Width = 30
        };

        var btnCompression = new Button("4. Data Compression/Decompression")
        {
            X = Pos.Center(),
            Y = 18,
            Width = 30
        };

        var btnSystemInfo = new Button("5. System Information")
        {
            X = Pos.Center(),
            Y = 20,
            Width = 30
        };

        var btnExit = new Button("6. Exit")
        {
            X = Pos.Center(),
            Y = 22,
            Width = 30
        };

        btnPasswordGen.Clicked += () => ShowPasswordGen();
        btnEncryption.Clicked += () => { };
        btnHealthCheck.Clicked += () => { };
        btnCompression.Clicked += () => { };
        btnSystemInfo.Clicked += () => { };
        btnExit.Clicked += () => Application.RequestStop();

        mainWindow.Add(header, btnPasswordGen, btnEncryption, btnHealthCheck, btnCompression, btnSystemInfo, btnExit);
        Application.Top.Add(mainWindow);
    }

    private static void ShowPasswordGen()
    {
        Application.Top.RemoveAll();

        var window = new Window("Secure Password Generation")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var lblLength = new Label("Password Length (min recommended: 17):")
        {
            X = 1,
            Y = 1,
            Width = 40
        };

        var txtLength = new TextField("17")
        {
            X = 1,
            Y = 2,
            Width = 20
        };

        var lblStrength = new Label("Password Strength:")
        {
            X = 1,
            Y = 4,
            Width = 40
        };

        int selectedStrength = 1;
        var rbStrength1 = new CheckBox(1, 5, "1. Absurdly Strong (!WARNING: Anything stronger may not usable)", true);
        var rbStrength2 = new CheckBox(1, 6, "2. Even Mightier", false);
        var rbStrength3 = new CheckBox(1, 7, "3. Dexter's Special Brew", false);
        
        rbStrength1.Toggled += (prev) => { if (rbStrength1.Checked) { selectedStrength = 1; rbStrength2.Checked = false; rbStrength3.Checked = false; } };
        rbStrength2.Toggled += (prev) => { if (rbStrength2.Checked) { selectedStrength = 2; rbStrength1.Checked = false; rbStrength3.Checked = false; } };
        rbStrength3.Toggled += (prev) => { if (rbStrength3.Checked) { selectedStrength = 3; rbStrength1.Checked = false; rbStrength2.Checked = false; } };

        var lblOptions = new Label("Options:")
        {
            X = 1,
            Y = 9,
            Width = 40
        };

        var chkUppercase = new CheckBox(1, 10, "Uppercase Letters", false);
        var chkNumbers = new CheckBox(1, 11, "Numbers", false);
        var chkSpecial = new CheckBox(1, 12, "Special Characters", false);
        var chkExcludeAmbiguous = new CheckBox(1, 13, "Exclude Visually Ambiguous Characters", false);
        var chkExcludeDupes = new CheckBox(1, 14, "Exclude duplicate Characters", false);
        var chkExcludeSeq = new CheckBox(1, 15, "Exclude Sequential Characters", false);

        var btnGenerate = new Button("Generate Password")
        {
            X = 1,
            Y = 17,
            Width = 20
        };

        var btnBack = new Button("Back to Main Menu")
        {
            X = 25,
            Y = 17,
            Width = 20
        };

        var lblPassword = new Label("")
        {
            X = 1,
            Y = 19,
            Width = Dim.Fill() - 2,
            Height = 3,
            ColorScheme = new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black) }
        };

        btnGenerate.Clicked += () =>
        {
            try
            {
                if (!int.TryParse(txtLength.Text.ToString(), out int length) || length < 1)
                {
                    MessageBox.ErrorQuery("Error", "Please enter a valid password length", "OK");
                    return;
                }

                int strength = selectedStrength;

                List<string> options = new();
                if (chkUppercase.Checked) options.Add("Uppercase Letters");
                if (chkNumbers.Checked) options.Add("Numbers");
                if (chkSpecial.Checked) options.Add("Special Characters");
                if (chkExcludeAmbiguous.Checked) options.Add("Exclude Visually Ambiguous Characters");
                if (chkExcludeDupes.Checked) options.Add("Exclude duplicate Characters");
                if (chkExcludeSeq.Checked) options.Add("Exclude Sequential Characters");

                currentPassword = DxPassword.GeneratePassword(length, strength, options);
                ShowSubMenu(currentPassword);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to generate password: {ex.Message}", "OK");
            }
        };

        btnBack.Clicked += () => ShowMainMenu();
        

        window.Add(lblLength, txtLength, lblStrength, rbStrength1, rbStrength2, rbStrength3,
                   lblOptions, chkUppercase, chkNumbers, chkSpecial, chkExcludeAmbiguous,
                   chkExcludeDupes, chkExcludeSeq, btnGenerate, btnBack, lblPassword);

        Application.Top.Add(window);
    }

    private static void ShowSubMenu(string password)
    {
        Application.Top.RemoveAll();

        var window = new Window("Password Menu")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var lblPassword = new Label($"Generated Password: {password}")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = 2,
            ColorScheme = new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black) }
        };

        var btnGenerateAnother = new Button("1. Generate Another Password")
        {
            X = Pos.Center(),
            Y = 4,
            Width = 30
        };

        var btnSave = new Button("2. Save Previously Generated Password")
        {
            X = Pos.Center(),
            Y = 6,
            Width = 30
        };

        var btnMainMenu = new Button("3. Main Menu")
        {
            X = Pos.Center(),
            Y = 8,
            Width = 30
        };

        var btnExit = new Button("4. Exit")
        {
            X = Pos.Center(),
            Y = 10,
            Width = 30
        };

        btnGenerateAnother.Clicked += () => ShowPasswordGen();
        btnSave.Clicked += () => ShowHashSelection(password);
        btnMainMenu.Clicked += () => ShowMainMenu();
        btnExit.Clicked += () => Application.RequestStop();

        window.Add(lblPassword, btnGenerateAnother, btnSave, btnMainMenu, btnExit);
        Application.Top.Add(window);
    }

    private static void ShowHashSelection(string password)
    {
        Application.Top.RemoveAll();

        var window = new Window("How Should Dexter hide your Secret?")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var lblTitle = new Label("Select Hashing Algorithm:")
        {
            X = 1,
            Y = 1,
            Width = 40
        };

        string selectedAlgorithm = "Argon2id";
        var rbArgon2 = new CheckBox(1, 3, "1. Argon2id", true);
        var rbSCrypt = new CheckBox(1, 4, "2. SCrypt", false);
        
        rbArgon2.Toggled += (prev) => { if (rbArgon2.Checked) { selectedAlgorithm = "Argon2id"; rbSCrypt.Checked = false; } };
        rbSCrypt.Toggled += (prev) => { if (rbSCrypt.Checked) { selectedAlgorithm = "SCrypt"; rbArgon2.Checked = false; } };

        var btnContinue = new Button("Continue")
        {
            X = 1,
            Y = 6,
            Width = 20
        };

        var btnCancel = new Button("Cancel")
        {
            X = 25,
            Y = 6,
            Width = 20
        };

        btnContinue.Clicked += () =>
        {
            try
            {
                string hash = DxPassword.GetHash(password, selectedAlgorithm);
                SaveAsync(hash);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to hash password: {ex.Message}", "OK");
            }
        };

        btnCancel.Clicked += () => ShowSubMenu(password);

        window.Add(lblTitle, rbArgon2, rbSCrypt, btnContinue, btnCancel);
        Application.Top.Add(window);
    }

    private static void SaveAsync(string hash)
    {
        Application.Top.RemoveAll();

        var window = new Window("Save Password")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var lblName = new Label("Name the password:")
        {
            X = 1,
            Y = 1,
            Width = 40
        };

        var txtName = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = 40
        };

        var lblStatus = new Label("")
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill() - 2,
            Height = 2
        };

        var btnSave = new Button("Save")
        {
            X = 1,
            Y = 7,
            Width = 20
        };

        var btnCancel = new Button("Cancel")
        {
            X = 25,
            Y = 7,
            Width = 20
        };

        btnSave.Clicked += async () =>
        {
            string? name = txtName.Text.ToString();
            var result = await DxPassword.SavePasswordAsync(name ?? "", hash);
            
            lblStatus.Text = result.Message;
            
            if (result.Success)
            {
                lblStatus.ColorScheme = new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black) };
                await Task.Delay(1500);
                ShowMainMenu();
            }
            else if (result.IsDuplicate)
            {
                lblStatus.ColorScheme = new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black) };
            }
            else
            {
                lblStatus.ColorScheme = new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.Red, Color.Black) };
            }
        };

        btnCancel.Clicked += () => ShowMainMenu();

        window.Add(lblName, txtName, lblStatus, btnSave, btnCancel);
        Application.Top.Add(window);
    }

    private static string GetHeaderText()
    {
        return @"
        
             /$$$$$$$                       /$$      
            | $$__  $$                     | $$      
            | $$  \ $$  /$$$$$$  /$$   /$$/$$$$$$    /$$$$$$   /$$$$$$\
            | $$  | $$ /$$__  $$ | $$ /$$/_  $$_/   /$$__  $$ /$$__  $$
            | $$  | $$| $$$$$$$$  \ $$$$/  | $$    | $$$$$$$$| $$  \__/
            | $$  | $$| $$_____/  >$$  $$  | $$ /$$| $$_____/| $$      
            | $$$$$$$/|  $$$$$$$ /$$/\  $$ |  $$$$/|  $$$$$$$| $$      
            |_______/  \_______/|__/  \__/  \___ /  \_______/|__/       v1.2";
    }
}
