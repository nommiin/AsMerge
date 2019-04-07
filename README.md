# AsMerge
An experimental C# program used to merge method names from one .NET assembly to another through very advanced guessing! I wasn't entirely happy with the results of this program as it didn't function as expected, but I will be attempting to make this program with a different approach.

# About
Essentially this program uses Mono.Cecil to read the "source" assembly and create a hash out of generic values such as the method paramenter types and first few instructions along it's name, then it reads the "target" assembly and gets compares the hash of every method to ones found in the "source" assembly and renames methods appropriately. You can also make sure the program only matches specific method names (via Regex) and adjust the amount of instructions to sample for the method hash.

# Dependencies 
* Mono.Cecil
