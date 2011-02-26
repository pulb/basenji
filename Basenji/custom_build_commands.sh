#!/bin/bash
# script to perform custom build steps
arg=$1
target_dir=$2

# gio-sharp is currenlty unstable and not installed into the GAC
gio_assembly="`pkg-config --variable=Libraries gio-sharp-2.0`"

case $arg in
	"--after-build")
		if [ ! -d $target_dir/data ]; then
			mkdir $target_dir/data
			cp -R images/basenji.svg images/themes $target_dir/data
		fi
		
		cp $gio_assembly $target_dir/
		cp $gio_assembly.config $target_dir
		;;
	"--after-clean")
		rm -rf $target_dir/data

		rm $target_dir/gio*
		;;
	*)
		echo "error: invalid argument."
		exit 1
esac
