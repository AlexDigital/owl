﻿using System;
using System.IO;

namespace owl
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Fields
			string input = "";
			string output = "";
			bool build_tree = false;
			bool beautify = true;
			bool validate = false;
			Verbosity.verb = VerbosityLevel.Basic;

			// Check the command-line arguments
			for (int i = 0; i < args.Length; i++) {
				switch (args [i]) {

				// Input file name
				case "-i":
				case "--input":
					if (args.Length >= ++i)
						input = args [i];
					break;

				// Output file name
				case "-o":
				case "--output":
					if (args.Length >= ++i)
						output = args [i];
					break;
				
				// Verbosity
				case "-v":
				case "--verb":
				case "--verbosity":
					if (args.Length >= ++i) {
						if (args [i].StartsWith ("-")) {
							Verbosity.verb = VerbosityLevel.Debug;
							i--;
							break;
						}
						VerbosityLevel verb;
						if (Enum.TryParse<VerbosityLevel> (args [i], out verb))
							Verbosity.verb = verb;
					}
					break;

				// Build token tree?
				case "--tree":
					build_tree = true;
					break;
				
				// Deactivate code beautification?
				case "--notidy":
					beautify = false;
					break;
				
				// Just validate the owl code?
				case "--validate":
					validate = true;
					break;
				
				// Undefined/Unsupported argument
				default:
					Log.Warning ("Unsupported argument '{0}'", args [i]);
					break;
				}
			}

			// Check if the user provided an input file
			if (input == "") {
				Log.Error ("Please specify an input file!\n\nUsage: owl -i [input]");
				return;
			}

			// Check if the user provided an output file
			// If not, set the output file to {input_file_name_without_extension}.html
			if (output == "")
				output = string.Format ("{0}.html", Path.GetFileNameWithoutExtension (input));

			// Do the lexical analysis
			Lexer lexer = new Lexer (input);
			lexer.Prepare ();
			Lexer.ErrorCode error = lexer.Scan ();

			// Check for lexer errors
			if ((int)error > 0) {
				if (validate)
				{
					Log.Write ("The owl code doesn't seem to be valid. Reason: {0}", Enum.GetName (typeof(Lexer.ErrorCode), error));
					return;
				}
				else {
					Log.Error ("The compilation didn't finish. Error: {0}", Enum.GetName (typeof(Lexer.ErrorCode), error));
					return;
				}
			}

			if (validate) {
				Log.Write ("Woop! Your owl code seems to be valid!");
				return;
			}

			// Build the tree if the --tree argument is given
			if (build_tree)
				lexer.BuildTree ();

			// Build the html code
			CodeGen generator = new CodeGen (lexer.GetTokens ());
			generator.Build ();

			// Beautify the output code
			if (beautify)
				generator.Beautify ();

			// Write the html code to disk
			generator.Serialize (output);

			Log.Write ("Done!");
		}
	}
}
