#/usr/local/bin/scss --default-encoding utf-8 --no-cache --update --style compressed ../css/"$entry".min.css

CommonPath=$(dirname `pwd`)
CssDirName=css
OutDir=../$CssDirName
mkdir -p $OutDir
for entry in *
do
  name=$(echo "$entry" | cut -f 1 -d '.')
  /usr/local/bin/scss --style compressed --no-cache $entry:$OutDir/$name.min.css
done
