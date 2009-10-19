
EXTRA_DIST = $(srcdir)/custom-hooks.make Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/Basenji.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Debug/Platform.dll \
	../VolumeDB/bin/Debug/VolumeDB.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
BASENJI_EXE_MDB_SOURCE=bin/Debug/Basenji.exe.mdb
BASENJI_EXE_MDB=$(BUILD_DIR)/Basenji.exe.mdb
VOLUMEDB_DLL_MDB_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
LIBEXTRACTOR_DLL_SOURCE=../LibExtractor/bin/Debug/LibExtractor.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
LIBEXTRACTOR_DLL_CONFIG_SOURCE=LibExtractor.dll.config
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = bin/Release/Basenji.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Release/Platform.dll \
	../VolumeDB/bin/Release/VolumeDB.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
BASENJI_EXE_MDB=
VOLUMEDB_DLL_MDB=
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Release/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
LIBEXTRACTOR_DLL_SOURCE=../LibExtractor/bin/Release/LibExtractor.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
LIBEXTRACTOR_DLL_CONFIG_SOURCE=LibExtractor.dll.config
PLATFORM_COMMON_DB_DLL_MDB=
HAL_DLL_MDB=
PLATFORM_DLL_MDB=

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/Basenji.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Debug/Platform.dll \
	../VolumeDB/bin/Debug/VolumeDB.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
BASENJI_EXE_MDB_SOURCE=bin/Debug/Basenji.exe.mdb
BASENJI_EXE_MDB=$(BUILD_DIR)/Basenji.exe.mdb
VOLUMEDB_DLL_MDB_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Debug/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
LIBEXTRACTOR_DLL_SOURCE=../LibExtractor/bin/Debug/LibExtractor.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
LIBEXTRACTOR_DLL_CONFIG_SOURCE=LibExtractor.dll.config
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = bin/Release/Basenji.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../Platform/bin/Release/Platform.dll \
	../VolumeDB/bin/Release/VolumeDB.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
BASENJI_EXE_MDB=
VOLUMEDB_DLL_MDB=
VOLUMEDB_DLL_SOURCE=../VolumeDB/bin/Release/VolumeDB.dll
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
LIBEXTRACTOR_DLL_SOURCE=../LibExtractor/bin/Release/LibExtractor.dll
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
LIBEXTRACTOR_DLL_CONFIG_SOURCE=LibExtractor.dll.config
PLATFORM_COMMON_DB_DLL_MDB=
HAL_DLL_MDB=
PLATFORM_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HAL_DLL) \
	$(BASENJI_EXE_MDB) \
	$(VOLUMEDB_DLL_MDB) \
	$(VOLUMEDB_DLL) \
	$(PLATFORM_COMMON_DB_DLL) \
	$(LIBEXTRACTOR_DLL) \
	$(PLATFORM_DLL) \
	$(LIBEXTRACTOR_DLL_CONFIG) \
	$(PLATFORM_COMMON_DB_DLL_MDB) \
	$(HAL_DLL_MDB) \
	$(PLATFORM_DLL_MDB)  

LINUX_DESKTOPAPPLICATIONS = \
	$(BASENJI_DESKTOP)  

BINARIES = \
	$(BASENJI)  


RESGEN=resgen2

HAL_DLL = $(BUILD_DIR)/Hal.dll
VOLUMEDB_DLL = $(BUILD_DIR)/VolumeDB.dll
BASENJI_DESKTOP = $(BUILD_DIR)/basenji.desktop
BASENJI = $(BUILD_DIR)/basenji
PLATFORM_COMMON_DB_DLL = $(BUILD_DIR)/Platform.Common.DB.dll
LIBEXTRACTOR_DLL = $(BUILD_DIR)/LibExtractor.dll
PLATFORM_DLL = $(BUILD_DIR)/Platform.dll
LIBEXTRACTOR_DLL_CONFIG = $(BUILD_DIR)/LibExtractor.dll.config

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
	src/S.cs \
	src/Gui/Widgets/CategoryView.cs \
	src/Gui/Widgets/PageNavigation.cs \
	src/Gui/Widgets/VolumeEditEvents.cs \
	src/Gui/Widgets/PageNavigationEvents.cs 

DATA_FILES = 

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
	System \
	-pkg:gtk-sharp-2.0 \
	-pkg:glib-sharp-2.0

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,HAL_DLL))
$(eval $(call emit-deploy-target,VOLUMEDB_DLL_MDB))
$(eval $(call emit-deploy-target,VOLUMEDB_DLL))
$(eval $(call emit-deploy-wrapper,BASENJI_DESKTOP,basenji.desktop))
$(eval $(call emit-deploy-wrapper,BASENJI,basenji))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL))
$(eval $(call emit-deploy-target,LIBEXTRACTOR_DLL))
$(eval $(call emit-deploy-target,PLATFORM_DLL))
$(eval $(call emit-deploy-target,LIBEXTRACTOR_DLL_CONFIG))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL_MDB))
$(eval $(call emit-deploy-target,HAL_DLL_MDB))
$(eval $(call emit-deploy-target,PLATFORM_DLL_MDB))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

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
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(HAL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(BASENJI_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(VOLUMEDB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(VOLUMEDB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(datadir)/applications'
	$(call cp,$(BASENJI_DESKTOP),$(DESTDIR)$(datadir)/applications)
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(BASENJI),$(DESTDIR)$(bindir))
	$(call cp,$(PLATFORM_COMMON_DB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LIBEXTRACTOR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(LIBEXTRACTOR_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_COMMON_DB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(HAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(HAL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(BASENJI_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(VOLUMEDB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(VOLUMEDB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(BASENJI_DESKTOP),$(DESTDIR)$(datadir)/applications)
	$(call rm,$(BASENJI),$(DESTDIR)$(bindir))
	$(call rm,$(PLATFORM_COMMON_DB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBEXTRACTOR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(LIBEXTRACTOR_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_COMMON_DB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(HAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
