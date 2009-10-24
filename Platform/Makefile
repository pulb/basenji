
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug -define:DEBUG "-define:UNIX,DEBUG"
ASSEMBLY = bin/Debug/Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Debug/Hal.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_MDB_SOURCE=bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:UNIX"
ASSEMBLY = bin/Release/Platform.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Release/Hal.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_MDB=
HAL_DLL_MDB=

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug -define:DEBUG -pkg:gnome-sharp-2.0  "-define:UNIX,DEBUG,GNOME"
ASSEMBLY = bin/Debug/Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Debug/Hal.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_MDB_SOURCE=bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ -pkg:gnome-sharp-2.0  "-define:UNIX,GNOME"
ASSEMBLY = bin/Release/Platform.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Release/Hal.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_MDB=
HAL_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HAL_DLL) \
	$(PLATFORM_DLL_MDB) \
	$(HAL_DLL_MDB)  


RESGEN=resgen2

HAL_DLL = $(BUILD_DIR)/Hal.dll

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
	src/Common/Globalization/TranslatedStringTable.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	System \
	Mono.Posix

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,HAL_DLL))
$(eval $(call emit-deploy-target,HAL_DLL_MDB))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


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
	$(call cp,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(HAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(HAL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(HAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
