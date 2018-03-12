
///Must have node.js installed
///Run script from command line with "node ConvertTypescriptCompilerToUseTabs.js"


var KnownPaths = [
	"C:\\Program Files (x86)\\Microsoft SDKs\\TypeScript\\2.6",
	"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Web\\External\\node_modules\\typescript\\lib"
];


var KnownFiles = [
	"typescriptServices.js",
	"typescript.js",
	"tsserverlibrary.js",
	"tsserver.js",
	"tsc.js"
];

var replaceText = 'var indentStrings = ["", "    "];';
var replacementText = 'var indentStrings = ["", "	"];';

const fs = require('fs');

KnownPaths.forEach(path => {
	var bkpPath = path +"\\bkp";
	if (!fs.existsSync(bkpPath)) {
		fs.mkdirSync(bkpPath);
	}
	KnownFiles.forEach(fileName => {
		var filePath = path + "\\" + fileName;
		var bkpFilePath = bkpPath + "\\" + fileName;
	});
});

if(!fs.existsSync)