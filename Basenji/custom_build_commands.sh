#!/bin/bash
# script to perform custom build steps
arg=$1
target_dir=$2
case $arg in
	"--after-build")
		if [ ! -d $target_dir/data ]; then
			mkdir $target_dir/data
			cp -R images/basenji.svg images/themes $target_dir/data
		fi
		;;
	"--after-clean")
		rm -rf $target_dir/data
		;;
	*)
		echo "error: invalid argument."
		exit 1
esac
