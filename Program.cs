using Ionic.Zip;

internal class Program
{
    //Dir
    private static readonly string dir = Directory.GetCurrentDirectory();
    private static readonly string extractDir = $"{dir}\\extract";

    const char FINAL = (char)173; //'¡'

    //Brute force function
    private static void brute(ref string success, ref bool found, ref byte[] data, string sample, int digits, string filename, 
        ref double percent, ref long max, ref double pVar, ref ushort currP)
    {
        //Calc the percent vars the first time
        if (max == 1 && String.IsNullOrEmpty(sample))
        {
            var cnt = digits;

            while (cnt-- > 0)
                max *= 141;

            percent = (double)max / 100;

            //Console.WriteLine($"Percent : {percent}\nMax: {max}\npVar: {pVar}\nCurrent: {currP}");

            //Console.ReadKey();
        }

        //Password to test
        var pass = string.Empty;

        //MemoryStream to check zipFile
        using (MemoryStream mem = new MemoryStream(data))
        {
            //Init char value
            char init = ' ';

            //Start test loop
            while (init < FINAL && !found)
            {
                pVar++;

                //Calc and show progress percent
                if ((ushort)(pVar / percent) > currP)
                    Console.WriteLine($"{(currP = (ushort)(pVar / percent))}% analyzed.");

                try
                {
                    pass = $"{sample}{init++}";

                    //Set MemoryStream Position
                    mem.Position = 0;

                    // extract entries that use encryption
                    if (ZipFile.CheckZipPassword(filename, pass))
                    {
                        success = pass;
                        found = true;
                    }

                    else
                    {
                        //If more digits to check
                        if (digits > 1)
                            //Recursive execution
                            brute(ref success, ref found, ref data, pass, digits - 1, filename,
                                ref percent, ref max, ref pVar, ref currP);
                    }
                }
                catch /*(Exception ex)*/
                {
                    //Console.WriteLine($"\nError v1 : {ex.Message} [{pass}] [POS : {mem.Position}]");

                    //if (ex.Message != "The password did not match.")
                    //{
                    //    //Console.WriteLine($"\nError v1 : {ex.Message} [{pass}] [POS : {mem.Position}]");

                    //    //break;
                    //}
                }
            }
        }
    }

    //Main function
    private static void Main(string[] args)
    {
        //SETUP
        Console.WriteLine("\nZIP password hack Bruteforce by Starlyn1232: ");

        var ERROR = 0;

        Console.Write("\nFilename: ");        
        string? file = Console.ReadLine();

        var PASS_DIGITS = 0;

        try
        {
            if (String.IsNullOrEmpty(file) || !File.Exists(file))
                throw new Exception();

            ERROR++;

            Console.Write("Password max length: ");
            PASS_DIGITS = Convert.ToInt32(Console.ReadLine());

            //From 1 to 128 characters (The more larger, it'll take 141 more time to accomplish)
            if (PASS_DIGITS < 1 || PASS_DIGITS > 128)
                throw new Exception();
        }
        catch
        {
            var error = string.Empty;

            switch (ERROR)
            {
                case 0:
                    error = "\nFile not found!";
                    break;
                case 1:
                    error = "\nInvalid password length!";
                    break;
                default:
                    error = "\nInvalid value!";
                    break;
            }

            ExitMsg(error);
        }

        //Init msg
        Console.WriteLine("\nBruteforce attack started!");

        //Warn about calc duration
        if (PASS_DIGITS > 4)
        {
            Console.WriteLine("\nCalculation ETA stats: (Runned on 'AMD Ryzen 9 5900HX')\n");

            decimal mins = 0.033M;

            int demo = 1, hour = 0;

            bool calcHour = false;

            do
            {
                Console.Write($"[{demo} digits] - {mins} minutes. ");
                Console.WriteLine((calcHour = (hour = (int)(mins / 60)) > 0) ? $"[{hour} hours]" : "");

                //Why multiply by 141? Because we're testing exactly 141/255 bytes posibilities from ASCII, starting
                //from 132 (' ') to the 173 ('¡'),
                mins *= 141;
            }

            while (++demo < 6);

            Console.Write("\nThe passworld calculation could take several minutes, are you sure of continue? (y/n): ");

            var ask = Console.ReadLine();

            if (String.IsNullOrEmpty(ask) || ask.ToLower() != "y")
                return;
        }

        //Save current time for ETA calc
        var now = DateTime.Now;

        try
        {
            //Prepare variables
            bool found = false;
            var success = string.Empty;

            var data = File.ReadAllBytes(file);
            var len = data.Length;

            //Check file len
            if (len == 0)
                ExitMsg("\nInvalid file length!");

            Console.WriteLine($"\nFile size : {len / 1024} KB\n");

            //Check if is zipFile
            if (!ZipFile.IsZipFile(file))
                ExitMsg("\nIs NOT a zip file!");

            //Clear extract dir
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            //Running engine!!!
            Console.WriteLine("\nRunning the engine!\n");

            //Percent calculation vars
            //double percent, ref long max, ref double pVar, ref UInt16 currP
            double percent = 0, pVar = 0;
            long max = 1;
            ushort currP = 0;

            //Recursive execution (Using password len)
            var t = new Thread(() => brute(ref success, ref found, ref data, "", PASS_DIGITS, file,
                ref percent, ref max, ref pVar, ref currP));

            //Multi-thread run
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();
            t.Join();

            //Check result
            if (found)
                // '\a' to alert you :)
                Console.WriteLine($"\nPassword found : '{success}'\n\a");

            else
                Console.WriteLine("\nPassword NOT found!");

            //ETA
            var m = (DateTime.Now - now).TotalSeconds;

            if (m > 59)
                Console.WriteLine($"\nTook time : {m / 60} mins.");

            else
                Console.WriteLine($"\nTook time : {m} seconds.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError v2 : {ex.Message}");
        }

        Console.ReadKey();
    }

    private static void ExitMsg(string txt)
    {
        Console.WriteLine("\nInvalid file length!");
        Console.ReadKey();

        Environment.Exit(1);
    }
}
