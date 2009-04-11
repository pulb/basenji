
EXTRA_DIST = $(srcdir)/custom-hooks.make Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -langversion:linq  "-define:DEBUG"

ASSEMBLY = bin/Debug/Basenji.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Debug/Platform.dll \
	../VolumeDB/bin/Debug/VolumeDB.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb
VOLUMEDB_DLL_MDB_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -langversion:linq 
ASSEMBLY = bin/Release/Basenji.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Release/Platform.dll \
	../VolumeDB/bin/Release/VolumeDB.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Release/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
HAL_DLL_MDB=
VOLUMEDB_DLL_MDB=

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -langversion:linq  "-define:DEBUG"

ASSEMBLY = bin/Debug/Basenji.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Debug/Platform.dll \
	../VolumeDB/bin/Debug/VolumeDB.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb
VOLUMEDB_DLL_MDB_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -langversion:linq 
ASSEMBLY = bin/Release/Basenji.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Release/Platform.dll \
	../VolumeDB/bin/Release/VolumeDB.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Release/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
HAL_DLL_MDB=
VOLUMEDB_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(HAL_DLL) \
	$(PLATFORM_DLL) \
	$(VOLUMEDB_DLL) \
	$(PLATFORM_COMMON_DB_DLL_MDB) \
	$(PLATFORM_DLL_MDB) \
	$(PLATFORM_COMMON_DB_DLL) \
	$(HAL_DLL_MDB) \
	$(VOLUMEDB_DLL_MDB)  

LINUX_DESKTOPAPPLICATIONS = \
	$(BASENJI_DESKTOP)  

BINARIES = \
	$(BASENJI)  



HAL_DLL = $(BUILD_DIR)/Hal.dll
PLATFORM_DLL = $(BUILD_DIR)/Platform.dll
VOLUMEDB_DLL = $(BUILD_DIR)/VolumeDB.dll
BASENJI_DESKTOP = $(BUILD_DIR)/basenji.desktop
PLATFORM_COMMON_DB_DLL = $(BUILD_DIR)/Platform.Common.DB.dll
BASENJI = $(BUILD_DIR)/basenji

FILES = \
	src/Gui/About.cs \
	src/Gui/DBProperties.cs \
	src/Gui/DriveSelection.cs \
	src/Gui/Events.cs \
	src/Gui/ItemSearch.cs \
	src/Gui/MainWindow.cs \
	src/Gui/Preferences.cs \
	src/Gui/VolumeProperties.cs \
	src/Gui/VolumeScanner.cs \
	src/Gui/Base/BinBase.cs \
	src/Gui/Base/DialogBase.cs \
	src/Gui/Base/ViewBase.cs \
	src/Gui/Base/WindowBase.cs \
	src/Gui/Widgets/DateChooser.cs \
	src/Gui/Widgets/Events.cs \
	src/Gui/Widgets/FSVolumeEdit.cs \
	src/Gui/Widgets/ItemInfo.cs \
	src/Gui/Widgets/ItemView.cs \
	src/Gui/Widgets/Led.cs \
	src/Gui/Widgets/SearchResultView.cs \
	src/Gui/Widgets/VolumeEdit.cs \
	src/Gui/Widgets/VolumeView.cs \
	src/Icons/CustomIconTheme.cs \
	src/Icons/Icon.cs \
	src/Icons/IconCache.cs \
	src/Icons/IconUtils.cs \
	src/Icons/ItemIcons.cs \
	src/Icons/MimeIconCache.cs \
	src/Icons/NameInCustomIconNameAttribute.cs \
	src/App.cs \
	src/AssemblyInfo.cs \
	src/DbData.cs \
	src/FileDialog.cs \
	src/Main.cs \
	src/MsgDialog.cs \
	src/Settings.cs \
	src/Util.cs \
	src/S.cs 

DATA_FILES = \
	app.desktop \
	basenji 

RESOURCES = \
	images/LED_Off.png \
	images/LED_On.png \
	po/de.resources \
	po/he.resources \
	po/da.resources \
	images/search.png 

EXTRAS = \
	basenji.desktop.in \
	basenji.in 

REFERENCES =  \
	-pkg:gtk-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:glade-sharp-2.0 \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,HAL_DLL))
$(eval $(call emit-deploy-target,PLATFORM_DLL))
$(eval $(call emit-deploy-target,VOLUMEDB_DLL))
$(eval $(call emit-deploy-wrapper,BASENJI_DESKTOP,basenji.desktop))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL_MDB))
$(eval $(call emit-deploy-target,PLATFORM_DLL_MDB))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL))
$(eval $(call emit-deploy-wrapper,BASENJI,basenji))
$(eval $(call emit-deploy-target,HAL_DLL_MDB))
$(eval $(call emit-deploy-target,VOLUMEDB_DLL_MDB))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'
# Targets for Custom commands
DEBUG_AfterBuild:
	(cd $(srcdir) && ./custom_build_commands.sh --after-build $(BUILD_DIR))

DEBUG_AfterClean:
	(cd $(srcdir) && ./custom_build_commands.sh --after-clean $(BUILD_DIR))

RELEASE_AfterBuild:
	(cd $(srcdir) && ./custom_build_commands.sh --after-build $(BUILD_DIR))

RELEASE_AfterClean:
	(cd $(srcdir) && ./custom_build_commands.sh --after-clean $(BUILD_DIR))

GNOME_DEBUG_AfterBuild:
	(cd $(srcdir) && ./custom_build_commands.sh --after-build $(BUILD_DIR))

GNOME_DEBUG_AfterClean:
	(cd $(srcdir) && ./custom_build_commands.sh --after-clean $(BUILD_DIR))

GNOME_RELEASE_AfterBuild:
	(cd $(srcdir) && ./custom_build_commands.sh --after-build $(BUILD_DIR))

GNOME_RELEASE_AfterClean:
	(cd $(srcdir) && ./custom_build_commands.sh --after-clean $(BUILD_DIR))


$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(dir $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL) $(VOLUMEDB_DLL) $(BASENJI_DESKTOP) $(PLATFORM_COMMON_DB_DLL_MDB) $(PLATFORM_DLL_MDB) $(PLATFORM_COMMON_DB_DLL) $(BASENJI) $(HAL_DLL_MDB) $(VOLUMEDB_DLL_MDB)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(libdir)/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(HAL_DLL)' || cp $(HAL_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_DLL)' || cp $(PLATFORM_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(VOLUMEDB_DLL)' || cp $(VOLUMEDB_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(datadir)/applications
	test -z '$(BASENJI_DESKTOP)' || cp $(BASENJI_DESKTOP) $(DESTDIR)$(datadir)/applications
	test -z '$(PLATFORM_COMMON_DB_DLL_MDB)' || cp $(PLATFORM_COMMON_DB_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_DLL_MDB)' || cp $(PLATFORM_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_COMMON_DB_DLL)' || cp $(PLATFORM_COMMON_DB_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(bindir)
	test -z '$(BASENJI)' || cp $(BASENJI) $(DESTDIR)$(bindir)
	test -z '$(HAL_DLL_MDB)' || cp $(HAL_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(VOLUMEDB_DLL_MDB)' || cp $(VOLUMEDB_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL) $(VOLUMEDB_DLL) $(BASENJI_DESKTOP) $(PLATFORM_COMMON_DB_DLL_MDB) $(PLATFORM_DLL_MDB) $(PLATFORM_COMMON_DB_DLL) $(BASENJI) $(HAL_DLL_MDB) $(VOLUMEDB_DLL_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(HAL_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL))
	test -z '$(PLATFORM_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_DLL))
	test -z '$(VOLUMEDB_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(VOLUMEDB_DLL))
	test -z '$(BASENJI_DESKTOP)' || rm -f $(DESTDIR)$(datadir)/applications/$(notdir $(BASENJI_DESKTOP))
	test -z '$(PLATFORM_COMMON_DB_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_COMMON_DB_DLL_MDB))
	test -z '$(PLATFORM_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_DLL_MDB))
	test -z '$(PLATFORM_COMMON_DB_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_COMMON_DB_DLL))
	test -z '$(BASENJI)' || rm -f $(DESTDIR)$(bindir)/$(notdir $(BASENJI))
	test -z '$(HAL_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL_MDB))
	test -z '$(VOLUMEDB_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(VOLUMEDB_DLL_MDB))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
