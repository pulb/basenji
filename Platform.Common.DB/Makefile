
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-define:DEBUG,UNIX"

ASSEMBLY = bin/Debug/Platform.Common.DB.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug


endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:UNIX"

ASSEMBLY = bin/Release/Platform.Common.DB.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release


endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

LINUX_PKGCONFIG = \
	$(PLATFORM_COMMON_DB_PC)  



PLATFORM_COMMON_DB_PC = $(BUILD_DIR)/platform.common.db.pc

FILES = \
	src/AssemblyInfo.cs \
	src/SqliteDB.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	platform.common.db.pc.in 

REFERENCES =  \
	System \
	System.Data \
	Mono.Data.Sqlite

DLL_REFERENCES = 

CLEANFILES = $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,PLATFORM_COMMON_DB_PC,platform.common.db.pc))


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

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(PLATFORM_COMMON_DB_PC)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(libdir)/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(libdir)/$(PACKAGE)
	mkdir -p $(DESTDIR)$(libdir)/pkgconfig
	test -z '$(PLATFORM_COMMON_DB_PC)' || cp $(PLATFORM_COMMON_DB_PC) $(DESTDIR)$(libdir)/pkgconfig
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(PLATFORM_COMMON_DB_PC)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(libdir)/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(PLATFORM_COMMON_DB_PC)' || rm -f $(DESTDIR)$(libdir)/pkgconfig/$(notdir $(PLATFORM_COMMON_DB_PC))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
