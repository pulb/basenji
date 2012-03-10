
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug -define:DEBUG "-define:UNIX,DEBUG"
ASSEMBLY = bin/Debug/Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

PLATFORM_DLL_MDB_SOURCE=bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:UNIX"
ASSEMBLY = bin/Release/Platform.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

PLATFORM_DLL_MDB=
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug -define:DEBUG -pkg:gtk-sharp-2.0 -pkg:gio-sharp-2.0  "-define:UNIX,DEBUG,GNOME"
ASSEMBLY = bin/Debug/Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

PLATFORM_DLL_MDB_SOURCE=bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ -pkg:gtk-sharp-2.0 -pkg:gio-sharp-2.0  "-define:UNIX,GNOME"
ASSEMBLY = bin/Release/Platform.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

PLATFORM_DLL_MDB=
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(PLATFORM_DLL_MDB) \
	$(PLATFORM_DLL_CONFIG)  

LINUX_PKGCONFIG = \
	$(PLATFORM_PC)  


RESGEN=resgen2

PLATFORM_DLL_CONFIG = $(BUILD_DIR)/Platform.dll.config
PLATFORM_PC = $(BUILD_DIR)/platform.pc

FILES = \
	src/Common/Diagnostics/CurrentPlatform.cs \
	src/Common/Diagnostics/CurrentRuntime.cs \
	src/Common/Diagnostics/Debug.cs \
	src/Common/IO/DriveInfo.cs \
	src/Common/IO/DriveNotMountedException.cs \
	src/Common/IO/DriveNotReadyException.cs \
	src/Common/IO/DriveType.cs \
	src/Common/IO/FileHelper.cs \
	src/Common/Mime/MimeIconLookup.cs \
	src/Common/Mime/MimeType.cs \
	src/Unix/IO/UnixFileHelper.cs \
	src/Unix/Mime/Mime.cs \
	src/Win32/Mime/RegistryMime.cs \
	src/AssemblyInfo.cs \
	src/Common/ThumbnailGenerator.cs \
	src/Gnome/ThumbnailGenerator.cs \
	src/Common/Globalization/Catalog.cs \
	src/Common/Globalization/TranslatedStringTable.cs \
	src/Win32/ThumbnailCreator.cs \
	src/Win32/ThumbnailGenerator.cs \
	src/Common/Diagnostics/AssertionFailedException.cs \
	src/Unix/IO/DkDisk.cs \
	src/Common/IO/IDriveInfoProvider.cs \
	src/Unix/IO/DkDriveInfoProvider.cs \
	src/Win32/IO/Win32DriveInfoProvider.cs \
	src/Win32/IO/AudioCdWin32.cs \
	src/Gnome/IO/GioDriveInfoProvider.cs \
	src/Gnome/GnomeDesktop/DesktopThumbnailFactory.cs \
	src/Gnome/GnomeDesktop/DesktopThumbnailSize.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Platform.dll.config \
	platform.pc.in 

REFERENCES =  \
	System \
	Mono.Posix \
	System.Core \
	-pkg:dbus-sharp-1.0 \
	-pkg:dbus-sharp-glib-1.0

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,PLATFORM_DLL_CONFIG))
$(eval $(call emit-deploy-wrapper,PLATFORM_PC,platform.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
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
	$(call cp,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(PLATFORM_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
