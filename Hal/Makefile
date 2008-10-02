
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"

ASSEMBLY = bin/Debug/Hal.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug


endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/Hal.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release


endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

LINUX_PKGCONFIG = \
	$(HAL_PC)  



HAL_PC = $(BUILD_DIR)/hal.pc

FILES = \
	src/AssemblyInfo.cs \
	src/Device.cs \
	src/Manager.cs \
	src/Volume.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	hal.pc.in 

REFERENCES =  \
	System \
	-pkg:ndesk-dbus-1.0 \
	-pkg:ndesk-dbus-glib-1.0

DLL_REFERENCES = 

CLEANFILES = $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,HAL_PC,hal.pc))


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

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_PC)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(libdir)/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(libdir)/pkgconfig
	test -z '$(HAL_PC)' || cp $(HAL_PC) $(DESTDIR)$(libdir)/pkgconfig
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(HAL_PC)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(HAL_PC)' || rm -f $(DESTDIR)$(libdir)/pkgconfig/$(notdir $(HAL_PC))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
