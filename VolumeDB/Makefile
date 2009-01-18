
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"

ASSEMBLY = bin/Debug/VolumeDB.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll \
	../Platform/bin/Debug/Platform.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/VolumeDB.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Release/Platform.Common.DB.dll \
	../Platform/bin/Release/Platform.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
HAL_DLL_MDB=

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"

ASSEMBLY = bin/Debug/VolumeDB.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll \
	../Platform/bin/Debug/Platform.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/VolumeDB.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Release/Platform.Common.DB.dll \
	../Platform/bin/Release/Platform.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
HAL_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(HAL_DLL) \
	$(PLATFORM_DLL) \
	$(PLATFORM_COMMON_DB_DLL_MDB) \
	$(PLATFORM_DLL_MDB) \
	$(PLATFORM_COMMON_DB_DLL) \
	$(HAL_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(VOLUMEDB_PC)  



HAL_DLL = $(BUILD_DIR)/Hal.dll
PLATFORM_DLL = $(BUILD_DIR)/Platform.dll
VOLUMEDB_PC = $(BUILD_DIR)/volumedb.pc
PLATFORM_COMMON_DB_DLL = $(BUILD_DIR)/Platform.Common.DB.dll

FILES = \
	src/Searching/CompareOperator.cs \
	src/Searching/FileSizeSearchCriteria.cs \
	src/Searching/FreeTextSearchCriteria.cs \
	src/Searching/FreeTextSearchField.cs \
	src/Searching/IDSearchCriteria.cs \
	src/Searching/IDSearchField.cs \
	src/Searching/ISearchCriteria.cs \
	src/Searching/MatchRule.cs \
	src/Searching/SearchCriteriaGroup.cs \
	src/Searching/TextCompareOperator.cs \
	src/VolumeScanner/Events.cs \
	src/VolumeScanner/FilesystemVolumeInfo.cs \
	src/VolumeScanner/FilesystemVolumeScanner.cs \
	src/VolumeScanner/IVolumeScanner.cs \
	src/VolumeScanner/VolumeInfo.cs \
	src/VolumeScanner/VolumeProber.cs \
	src/VolumeScanner/VolumeScannerBase.cs \
	src/AssemblyInfo.cs \
	src/BufferedVolumeItemWriter.cs \
	src/DatabaseProperties.cs \
	src/DirectoryVolumeItem.cs \
	src/FieldnameValuePair.cs \
	src/FileSystemVolume.cs \
	src/FileSystemVolumeItem.cs \
	src/FileVolumeItem.cs \
	src/IChildItem.cs \
	src/IContainerItem.cs \
	src/IHashable.cs \
	src/IRecordData.cs \
	src/IVolumeDBRecord.cs \
	src/UnsupportedDbVersionException.cs \
	src/Util.cs \
	src/Volume.cs \
	src/VolumeDatabase.cs \
	src/VolumeDatabase.SqlBackend.cs \
	src/VolumeDBDataType.cs \
	src/VolumeDriveType.cs \
	src/VolumeItem.cs \
	src/VolumeItemType.cs \
	src/VolumeType.cs \
	src/S.cs 

DATA_FILES = 

RESOURCES = \
	po/de.resources \
	po/he.resources \
	po/da.resources 

EXTRAS = \
	volumedb.pc.in 

REFERENCES =  \
	System \
	System.Data

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,HAL_DLL))
$(eval $(call emit-deploy-target,PLATFORM_DLL))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL_MDB))
$(eval $(call emit-deploy-target,PLATFORM_DLL_MDB))
$(eval $(call emit-deploy-wrapper,VOLUMEDB_PC,volumedb.pc))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL))
$(eval $(call emit-deploy-target,HAL_DLL_MDB))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(dir $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL) $(PLATFORM_COMMON_DB_DLL_MDB) $(PLATFORM_DLL_MDB) $(VOLUMEDB_PC) $(PLATFORM_COMMON_DB_DLL) $(HAL_DLL_MDB)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(libdir)/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(HAL_DLL)' || cp $(HAL_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_DLL)' || cp $(PLATFORM_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_COMMON_DB_DLL_MDB)' || cp $(PLATFORM_COMMON_DB_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_DLL_MDB)' || cp $(PLATFORM_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(libdir)/pkgconfig
	test -z '$(VOLUMEDB_PC)' || cp $(VOLUMEDB_PC) $(DESTDIR)$(libdir)/pkgconfig
	test -z '$(PLATFORM_COMMON_DB_DLL)' || cp $(PLATFORM_COMMON_DB_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(HAL_DLL_MDB)' || cp $(HAL_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL) $(PLATFORM_COMMON_DB_DLL_MDB) $(PLATFORM_DLL_MDB) $(VOLUMEDB_PC) $(PLATFORM_COMMON_DB_DLL) $(HAL_DLL_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(HAL_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL))
	test -z '$(PLATFORM_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_DLL))
	test -z '$(PLATFORM_COMMON_DB_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_COMMON_DB_DLL_MDB))
	test -z '$(PLATFORM_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_DLL_MDB))
	test -z '$(VOLUMEDB_PC)' || rm -f $(DESTDIR)$(libdir)/pkgconfig/$(notdir $(VOLUMEDB_PC))
	test -z '$(PLATFORM_COMMON_DB_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_COMMON_DB_DLL))
	test -z '$(HAL_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL_MDB))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
