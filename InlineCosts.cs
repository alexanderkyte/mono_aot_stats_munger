using System;
using System.IO;
using System.Json;
using System.Collections.Generic;

public class AOTStats {

	public static String MethodJoin(JsonObject method) {
		return method["signature"].ToString(); // method ["namespace"] + "_" + method ["class"] + "_" + method ["name"] + "_" + method ["signature"] + method ["wrapper_type"];
	}

	public static int MethodCost (JsonObject method) {
		return Int32.Parse (method ["code_size"]);
	}

	public static void Main (string[] args) {
		var costs = new Dictionary<string, int>();


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


		var inlined = new HashSet<String>();

		foreach (var arg in args) {
			using (StreamReader r = new StreamReader (arg)) {
				JsonObject total = (JsonObject)JsonObject.Load(r);
				foreach (JsonObject method in total ["methods"]) {
					var key = AOTStats.MethodJoin (method);
					var code_size = AOTStats.MethodCost (method);
					if (!costs.ContainsKey (key))
						costs.Add (key, code_size);
					if (method["wrapper_type"] != "none") {
						int prev = wrapper_type_costs[method["wrapper_type"]];
						wrapper_type_costs[method["wrapper_type"]] = prev + AOTStats.MethodCost (method);
					}
				}
				foreach (JsonObject inlined_method in total ["inlined_methods"]) {
					inlined.Add (AOTStats.MethodJoin (inlined_method));
				}
			}
		}

		int cost = 0;
		var missed = new HashSet<String>();

		Console.WriteLine("=======================================================");
		foreach (var paid in inlined) {
			try {
				cost += costs [paid];
			} catch (Exception) {
				Console.WriteLine ("Not seen but inlined: {0}", paid);
				missed.Add (paid);
			}
		}
		Console.WriteLine("=======================================================");

		Console.WriteLine("Inlining:");
		Console.WriteLine("Sum of code_size for inlined methods  == {0}", cost);
		Console.WriteLine("Missing data for {0} out of {1} inlined references",  missed.Count, inlined.Count);
		Console.WriteLine("=======================================================");

		Console.WriteLine("Wrapper Types:");

		foreach (var entry in wrapper_type_costs) {
			Console.WriteLine ("Wrapper type: {0} has cumulative code size {1}", entry.Key, entry.Value);
		}

		Console.WriteLine("=======================================================");
	}
}
