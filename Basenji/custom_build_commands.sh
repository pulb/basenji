#!/bin/bash
# script to perform custom build steps
arg=$1
target_dir=$2

# gio-sharp is currenlty unstable and not installed into the GAC
# so copy it localy
gio_assembly_path="`pkg-config --variable=Libraries gio-sharp-2.0`"
gio_assembly_name="`basename $gio_assembly_path`"

case $arg in
	"--after-build")
		if [ ! -d $target_dir/data ]; then
			mkdir $target_dir/data
			cp -R images/basenji.svg images/themes $target_dir/data
		fi
		
		if [ ! -f $target_dir/$gio_assembly_name ]; then
			cp $gio_assembly_path $target_dir
		fi

		if [ ! -f $target_dir/$gio_assembly_name.config ]; then		
			cp $gio_assembly_path.config $target_dir/
		fi
		;;
	"--after-clean")
		rm -rf $target_dir/data
	
		if [ -f $target_dir/$gio_assembly_name ]; then
			rm $target_dir/$gio_assembly_name
		fi
		if [ -f $target_dir/$gio_assembly_name.config ]; then
			rm $target_dir/$gio_assembly_name.config
		fi
		;;
	*)
		echo "error: invalid argument."
		exit 1
esac
