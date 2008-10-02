
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-define:UNIX,GNOME,DEBUG"

ASSEMBLY = bin/Debug/Platform.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Debug/Hal.dll
BUILD_DIR = bin/Debug

HAL_DLL_SOURCE=../Hal/bin/Debug/Hal.dll
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config
HAL_DLL_MDB_SOURCE=../Hal/bin/Debug/Hal.dll.mdb
HAL_DLL_MDB=$(BUILD_DIR)/Hal.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:UNIX,GNOME"

ASSEMBLY = bin/Release/Platform.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Hal/bin/Release/Hal.dll
BUILD_DIR = bin/Release

HAL_DLL_SOURCE=../Hal/bin/Release/Hal.dll
PLATFORM_DLL_CONFIG_SOURCE=Platform.dll.config
HAL_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(HAL_DLL) \
	$(PLATFORM_DLL_CONFIG) \
	$(HAL_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(PLATFORM_PC)  



HAL_DLL = $(BUILD_DIR)/Hal.dll
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
	src/Gnome/GnomeNative.cs \
	src/Unix/IO/UnixFileHelper.cs \
	src/Unix/Mime/Mime.cs \
	src/Win32/Mime/RegistryMime.cs \
	src/AssemblyInfo.cs 

DATA_FILES = \
	Platform.dll.config 

RESOURCES = 

EXTRAS = \
	platform.pc.in 

REFERENCES =  \
	System \
	Mono.Posix

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,HAL_DLL))
$(eval $(call emit-deploy-target,PLATFORM_DLL_CONFIG))
$(eval $(call emit-deploy-target,HAL_DLL_MDB))
$(eval $(call emit-deploy-wrapper,PLATFORM_PC,platform.pc))


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

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL_CONFIG) $(HAL_DLL_MDB) $(PLATFORM_PC)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(libdir)/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(HAL_DLL)' || cp $(HAL_DLL) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(PLATFORM_DLL_CONFIG)' || cp $(PLATFORM_DLL_CONFIG) $(DESTDIR)$(libdir)/$(PACKAGE)
	test -z '$(HAL_DLL_MDB)' || cp $(HAL_DLL_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(libdir)/pkgconfig
	test -z '$(PLATFORM_PC)' || cp $(PLATFORM_PC) $(DESTDIR)$(libdir)/pkgconfig
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_DLL) $(PLATFORM_DLL_CONFIG) $(HAL_DLL_MDB) $(PLATFORM_PC)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(HAL_DLL)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL))
	test -z '$(PLATFORM_DLL_CONFIG)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(PLATFORM_DLL_CONFIG))
	test -z '$(HAL_DLL_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(HAL_DLL_MDB))
	test -z '$(PLATFORM_PC)' || rm -f $(DESTDIR)$(libdir)/pkgconfig/$(notdir $(PLATFORM_PC))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
