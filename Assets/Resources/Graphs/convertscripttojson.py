import json
from os import listdir
from os.path import isfile, join

directory = "C:/Users/Patrick/vn/Assets/Resources/Graphs"

onlyfiles = [f for f in listdir(directory) if isfile(join(directory, f))]

onlyfiles = [f for f in onlyfiles if f[-11:] == "_script.txt"]#todo

for f in onlyfiles:
	dat = open(directory+"/"+f,"r").read().split("\n")
	if dat[0][0] == "_":
		dat = dat[1:]
	line_type = "node_name"
	line_num = 0
	node_list = []
	current_node = {"name":"", "dl":"", "outn":[], "outdl":[], "valskeys":[], "valsvals":[], "valsarrkeys":[], "valsarrvals":[]}
	print(dat)
	while line_num < len(dat):
		#making new nodes with any line starting with a '_'
		if (dat[line_num] == ""):
			line_num += 1
		elif (dat[line_num][0] == "_"):
			node_list.append(current_node.copy())
			current_node = {"name":"", "dl":"", "outn":[], "outdl":[], "valskeys":[], "valsvals":[], "valsarrkeys":[], "valsarrvals":[]}
			line_type = "node_name"
			line_num += 1
		#ignore lines starting with '#'
		elif (dat[line_num][0] == "#"):
			line_num += 1
		elif (line_type == "node_name"):
			current_node["name"] = dat[line_num]
			line_type = "dialogue"
			line_num += 1
		elif (line_type == "dialogue"):
			current_node["dl"] = dat[line_num]
			line_type = "info"
			line_num += 1
		elif (line_type == "info" and len(dat[line_num]) > 3 and dat[line_num][0:3].upper() == "SET"):
			spl = dat[line_num].split(" ")
			thingtoset = spl[1]
			if len(spl) > 2:
				val = spl[2]
				if (val == " " or val == ""):
					current_node["valskeys"].append(thingtoset)
					current_node["valsvals"].append("")
				else:
					isvalarray = val[0] == "[" and val[-1] == "]"
					if isvalarray:
						current_node["valsarrkeys"].append(thingtoset)
						current_node["valsarrvals"].append(str(val))
					else:
						current_node["valskeys"].append(thingtoset)
						current_node["valsvals"].append(val)
			else:
				current_node["valskeys"].append(thingtoset)
				current_node["valsvals"].append("")
			line_num += 1
		#adding outn
		elif (line_type == "info" and len(dat[line_num]) > 3 and dat[line_num][0:2] == "->"):
			current_node["outn"] = eval(dat[line_num][3:])
			line_num += 1
		#adding outdl
		elif (line_type == "info" and len(dat[line_num]) > 3 and dat[line_num][0:2] == "-:"):
			current_node["outdl"] = eval(dat[line_num][3:])
			line_num += 1
		else:
			print("could not register line number " + str(line_num) + ": " + dat[line_num])
			line_num += 1
	#final line adding last node
	node_list.append(current_node)

	fulljsonobj = {"callfunction":"","preq":["true"],"outgraphs":[],"data":node_list}
	with open(directory+"/"+f[:-11]+"_obj.json","w") as output:
		json.dump(fulljsonobj, output, indent=4)