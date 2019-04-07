using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;


namespace AsMerge {
    class Program {
        static void Main(string[] args) {
            // "source" contains the original (unobfuscated) type names, "target" is the obfuscated assembly you want to update, "extension" holds target extension
            string asSource = @"", asTarget = @"", asExtension = Path.GetExtension(asTarget);

            // "match" holds method name to match in "target", if null then every method will be read
            Regex asMatch = null; // Match de4dot deobfuscated methods: new Regex(@".?(method_\d)"); 

            // "count" is how many instructions to sample for the method hash, higher value = more matches, slower compute
            int asCount = 3;

            // Calculate hashes for each method in the source assembly
            Console.Write("Reading source assembly...");
            Dictionary<string, string> asHashes = new Dictionary<string, string>();
            using (ModuleDefinition asRead = ModuleDefinition.ReadModule(asSource)) {
                foreach(TypeDefinition asType in asRead.GetTypes()) {
                    foreach(MethodDefinition asMethod in asType.GetMethods()) {
                        string asSignature = "";
                        if (asMethod.HasParameters == true) {
                            foreach (ParameterDefinition asParameter in asMethod.Parameters) {
                                asSignature += asParameter.ParameterType.ToString();
                            }
                        }
                        // NOTE: Do not capture operands, as they could contain obfuscated values
                        if (asMethod.HasBody == true) {
                            for(byte i = 0; i < (asCount == -1 ? asMethod.Body.Instructions.Count : Math.Min(asMethod.Body.Instructions.Count, asCount)); i++) {
                                asSignature += asMethod.Body.Instructions[i].OpCode.ToString();
                            }
                        }
                        string asHash = GetHash(asSignature);
                        asHashes[asHash] = asMethod.Name;
                    }
                }
            }
            Console.WriteLine("DONE\nRead {0} methods.", asHashes.Count);

            // Read all methods in target assembly and compare hashes
            Console.Write("Backing up target assembly...");
            if (File.Exists(Path.GetFileNameWithoutExtension(asTarget) + ".bak" + asExtension) == false) File.Copy(asTarget, Path.GetFileNameWithoutExtension(asTarget) + ".bak" + asExtension);
            Console.Write("DONE\nReading target assembly...");
            int asMatched = 0;
            using (ModuleDefinition asRead = ModuleDefinition.ReadModule(asTarget)) {
                foreach (TypeDefinition asType in asRead.GetTypes()) {
                    foreach (MethodDefinition asMethod in asType.GetMethods()) {
                        if (asMatch != null && asMatch.Match(asMethod.Name).Success == false) continue;
                        string asSignature = "";
                        if (asMethod.HasParameters == true) {
                            foreach (ParameterDefinition asParameter in asMethod.Parameters) {
                                asSignature += asParameter.ParameterType.ToString();
                            }
                        }
                        // NOTE: Do not capture operands, as they could contain obfuscated values
                        if (asMethod.HasBody == true) {
                            for (byte i = 0; i < (asCount == -1 ? asMethod.Body.Instructions.Count : Math.Min(asMethod.Body.Instructions.Count, asCount)); i++) {
                                asSignature += asMethod.Body.Instructions[i].OpCode.ToString();
                            }
                        }
                        string asHash = GetHash(asSignature);
                        if (asHashes.ContainsKey(asHash) == true) {
                            asMethod.Name = asHashes[asHash];
                            asMatched++;
                        }
                    }
                }
                asRead.Write(Path.GetDirectoryName(asTarget) + "\\" + Path.GetFileNameWithoutExtension(asTarget) + "-Merged" + asExtension);
            }
            Console.WriteLine("DONE\nMatched {0} methods", asMatched);
            Console.ReadKey();
        }

        static string GetHash(string asInput) {
            string asHash = "";
            foreach(byte asByte in new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(asInput))) {
                asHash += asByte.ToString("X2");
            }
            return asHash;
        }
    }
}