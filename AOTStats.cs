using System;
using System.IO;
using System.Json;
using System.Collections.Generic;

public class InlineCosts {

	public static String MethodJoin(JsonObject method) {
		return method["signature"].ToString(); // method ["namespace"] + "_" + method ["class"] + "_" + method ["name"] + "_" + method ["signature"] + method ["wrapper_type"];
	}

	public static int MethodCost (JsonObject method) {
		return Int32.Parse (method ["code_size"]);
	}

	public static void Main (string[] args) {
		var wrapper_type_costs = new Dictionary<string, int>();
		wrapper_type_costs ["alloc"] = 0;
		wrapper_type_costs ["castclass"] = 0;
		wrapper_type_costs ["delegate-begin-invoke"] = 0;
		wrapper_type_costs ["delegate-end-invoke"] = 0;
		wrapper_type_costs ["delegate-invoke"] = 0;
		wrapper_type_costs ["isinst"] = 0;
		wrapper_type_costs ["managed-to-managed"] = 0;
		wrapper_type_costs ["managed-to-native"] = 0;
		wrapper_type_costs ["native-to-managed"] = 0;
		wrapper_type_costs ["runtime-invoke"] = 0;
		wrapper_type_costs ["stelemref"] = 0;
		wrapper_type_costs ["synchronized"] = 0;
		wrapper_type_costs ["unknown"] = 0;
		wrapper_type_costs ["write-barrier"] = 0;

		var inlined = new Dictionary<String, Tuple<int, int>>();
		var costs = new Dictionary<string, int>();
		var duplications = new Dictionary<string, int>();

		foreach (var arg in args) {
			Console.WriteLine("Processing {0}", arg);
			using (StreamReader r = new StreamReader (arg)) {
				JsonObject total = (JsonObject)JsonObject.Load(r);

				foreach (JsonObject method in total ["methods"]) {
					var key = InlineCosts.MethodJoin (method);
					var code_size = InlineCosts.MethodCost (method);
					if (!costs.ContainsKey (key)) {
						costs.Add (key, code_size);
					} else {
						if (duplications.ContainsKey (key)) {
							duplications[key] = duplications[key] + 1;
						} else {
							duplications[key] = 1;
						}
					}

					if (method["wrapper_type"] != "none") {
						int prev = wrapper_type_costs[method["wrapper_type"]];
						wrapper_type_costs[method["wrapper_type"]] = prev + InlineCosts.MethodCost (method);
					}
				}

				foreach (JsonObject inlined_method in total ["inlined_methods"]) {
					var key = InlineCosts.MethodJoin (inlined_method);
					var successes = Int32.Parse (inlined_method ["inline_successes"]);
					var failures = Int32.Parse (inlined_method ["inline_failures"]);

					if (inlined.ContainsKey (key)) {
						successes += inlined[key].Item1;
						failures += inlined[key].Item2;
					}

					inlined [key] = new Tuple<int, int>(successes, failures);
				}
			}
		}

		var cost_for_percent = new int[21];
		var missed = new HashSet<String>();

		Console.WriteLine("=======================================================");
		foreach (var entry in inlined) {
			try {
				int successes = entry.Value.Item1;
				int failures = entry.Value.Item2;
				int percentage = (successes * 100) / (successes + failures);
				int slot = percentage / 5;
				if (slot > 20 || slot < 1)
					throw new Exception ("Math error");
				cost_for_percent[slot] += costs [entry.Key];
			} catch (Exception e) {
				Console.WriteLine ("Not seen but inlined: {0}", entry.Key);
				missed.Add (entry.Key);
			}
		}
		Console.WriteLine("=======================================================");

		Console.WriteLine("Inlining:");
		Console.WriteLine("Sum of code_size for inlined methods:");
		for (int i=0; i < 21; i++) {
			Console.WriteLine("Methods inlined {0}% of the time have a cost of {1} bytes.", i * 5, cost_for_percent[i]);
		}
		Console.WriteLine("Missing data for {0} out of {1} inlined references",  missed.Count, inlined.Count);
		Console.WriteLine("=======================================================");

		Console.WriteLine("Wrapper Types:");

		foreach (var entry in wrapper_type_costs) {
			Console.WriteLine ("Wrapper type: {0} has cumulative code size {1}", entry.Key, entry.Value);
		}

		Console.WriteLine("=======================================================");
		Console.WriteLine("Generic duplication: (cost of duplicates only, not original) ");
		int total_dup_cost = 0;
		foreach (var entry in duplications) {
			//Console.WriteLine("{0} has {1} duplicates for a cost of {2}", entry.Key, entry.Value, entry.Value * costs[entry.Key]);
			total_dup_cost += entry.Value * costs[entry.Key];
		}
		Console.WriteLine ("Total duplicate cost: {0}", total_dup_cost);
		Console.WriteLine("=======================================================");
	}
}
