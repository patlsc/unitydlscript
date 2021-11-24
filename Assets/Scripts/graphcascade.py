#allows conversation json files to be small
#this has shitty performance but idc
import json
from os import listdir
from os.path import isfile, join

directory = "C:/Users/Patrick/vn/Assets/Resources/Graphs"

onlyfiles = [f for f in listdir(directory) if isfile(join(directory, f))]

onlyfiles = [f for f in onlyfiles if f[-9:] == "_min.json"]

def getbyname(whole:dict, name:str):
	for node in whole["data"]:
		if node["name"] == name:
			return node

def cascade_recurse(whole:dict,node:dict,seennames:set):
	if len(node["outn"]) == 0:
		return
	else:
		for outnodename in node["outn"]:
			outnode = getbyname(whole, outnodename)
			#overwrite empty fields of outnode with node's vals for it
			if not outnodename in seennames:
				seennames.add(node["name"])
				if outnode != None:
					for key in node:
						if key not in outnode:
							outnode[key] = node[key]
					cascade_recurse(whole, outnode, seennames)

def cascade(whole:dict):
	cascade_recurse(whole, getbyname(whole,"start"),set())

for f in onlyfiles:
	dat = json.load(open(directory+"/"+f,"r"))
	print(dat)
	cascade(dat)
	with open(directory+"/"+f[:-9]+"_cascade.json","w") as output:
		json.dump(dat, output, indent=4)