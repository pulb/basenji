
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/VolumeDB.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll \
	../Platform/bin/Debug/Platform.dll \
	../MusicBrainz/bin/Debug/MusicBrainz.dll
BUILD_DIR = bin/Debug

VOLUMEDB_DLL_MDB_SOURCE=bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
PLATFORM_DLL_CONFIG_SOURCE=../Platform/Platform.dll.config
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
MUSICBRAINZ_DLL_SOURCE=../MusicBrainz/bin/Debug/MusicBrainz.dll
MUSICBRAINZ_DLL_MDB_SOURCE=../MusicBrainz/bin/Debug/MusicBrainz.dll.mdb
MUSICBRAINZ_DLL_MDB=$(BUILD_DIR)/MusicBrainz.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = bin/Release/VolumeDB.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Release/Platform.Common.DB.dll \
	../Platform/bin/Release/Platform.dll \
	../MusicBrainz/bin/Release/MusicBrainz.dll
BUILD_DIR = bin/Release

VOLUMEDB_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
PLATFORM_DLL_CONFIG_SOURCE=../Platform/Platform.dll.config
PLATFORM_DLL_MDB=
MUSICBRAINZ_DLL_SOURCE=../MusicBrainz/bin/Release/MusicBrainz.dll
MUSICBRAINZ_DLL_MDB=

endif

ifeq ($(CONFIG),GNOME_DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/VolumeDB.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll \
	../Platform/bin/Debug/Platform.dll \
	../MusicBrainz/bin/Debug/MusicBrainz.dll
BUILD_DIR = bin/Debug

VOLUMEDB_DLL_MDB_SOURCE=bin/Debug/VolumeDB.dll.mdb
VOLUMEDB_DLL_MDB=$(BUILD_DIR)/VolumeDB.dll.mdb
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll
PLATFORM_COMMON_DB_DLL_MDB_SOURCE=../Platform.Common.DB/bin/Debug/Platform.Common.DB.dll.mdb
PLATFORM_COMMON_DB_DLL_MDB=$(BUILD_DIR)/Platform.Common.DB.dll.mdb
PLATFORM_DLL_SOURCE=../Platform/bin/Debug/Platform.dll
PLATFORM_DLL_CONFIG_SOURCE=../Platform/Platform.dll.config
PLATFORM_DLL_MDB_SOURCE=../Platform/bin/Debug/Platform.dll.mdb
PLATFORM_DLL_MDB=$(BUILD_DIR)/Platform.dll.mdb
MUSICBRAINZ_DLL_SOURCE=../MusicBrainz/bin/Debug/MusicBrainz.dll
MUSICBRAINZ_DLL_MDB_SOURCE=../MusicBrainz/bin/Debug/MusicBrainz.dll.mdb
MUSICBRAINZ_DLL_MDB=$(BUILD_DIR)/MusicBrainz.dll.mdb

endif

ifeq ($(CONFIG),GNOME_RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = bin/Release/VolumeDB.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Platform.Common.DB/bin/Release/Platform.Common.DB.dll \
	../Platform/bin/Release/Platform.dll \
	../MusicBrainz/bin/Release/MusicBrainz.dll
BUILD_DIR = bin/Release

VOLUMEDB_DLL_MDB=
PLATFORM_COMMON_DB_DLL_SOURCE=../Platform.Common.DB/bin/Release/Platform.Common.DB.dll
PLATFORM_COMMON_DB_DLL_MDB=
PLATFORM_DLL_SOURCE=../Platform/bin/Release/Platform.dll
PLATFORM_DLL_CONFIG_SOURCE=../Platform/Platform.dll.config
PLATFORM_DLL_MDB=
MUSICBRAINZ_DLL_SOURCE=../MusicBrainz/bin/Release/MusicBrainz.dll
MUSICBRAINZ_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(VOLUMEDB_DLL_MDB) \
	$(PLATFORM_COMMON_DB_DLL) \
	$(PLATFORM_COMMON_DB_DLL_MDB) \
	$(PLATFORM_DLL) \
	$(PLATFORM_DLL_CONFIG) \
	$(PLATFORM_DLL_MDB) \
	$(MUSICBRAINZ_DLL) \
	$(MUSICBRAINZ_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(VOLUMEDB_PC)  


RESGEN=resgen2

PLATFORM_COMMON_DB_DLL = $(BUILD_DIR)/Platform.Common.DB.dll
PLATFORM_DLL = $(BUILD_DIR)/Platform.dll
PLATFORM_DLL_CONFIG = $(BUILD_DIR)/Platform.dll.config
MUSICBRAINZ_DLL = $(BUILD_DIR)/MusicBrainz.dll
VOLUMEDB_PC = $(BUILD_DIR)/volumedb.pc

FILES = \
	src/Searching/CompareOperator.cs \
	src/Searching/ISearchCriteria.cs \
	src/Searching/MatchRule.cs \
	src/Searching/SearchCriteriaGroup.cs \
	src/Searching/TextCompareOperator.cs \
	src/VolumeScanner/Events.cs \
	src/VolumeScanner/FilesystemVolumeInfo.cs \
	src/VolumeScanner/FilesystemVolumeScanner.cs \
	src/VolumeScanner/IVolumeScanner.cs \
	src/VolumeScanner/VolumeInfo.cs \
	src/VolumeScanner/AbstractVolumeScanner.cs \
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
	src/S.cs \
	src/Searching/ExcludedSearchCriteria.cs \
	src/Searching/EUSL/Parsing/Collect.cs \
	src/Searching/EUSL/Parsing/EventHandlers.cs \
	src/Searching/EUSL/Parsing/Parser.cs \
	src/Searching/EUSL/Parsing/Relation.cs \
	src/Searching/EUSL/Parsing/TermType.cs \
	src/Searching/EUSL/Parsing/UnexpectedTokenException.cs \
	src/Searching/EUSL/Scanning/Scanner.cs \
	src/Searching/EUSL/Scanning/ScannerException.cs \
	src/Searching/EUSL/Scanning/Token.cs \
	src/Searching/EUSL/Scanning/TokenKind.cs \
	src/TooManyResultsException.cs \
	src/Searching/ItemSearchCriteria/FileSizeSearchCriteria.cs \
	src/Searching/ItemSearchCriteria/FreeTextSearchField.cs \
	src/Searching/ItemSearchCriteria/IDSearchCriteria.cs \
	src/Searching/ItemSearchCriteria/IDSearchField.cs \
	src/Searching/ItemSearchCriteria/MediaType.cs \
	src/Searching/ItemSearchCriteria/MediaTypeSearchCriteria.cs \
	src/Searching/SearchCriteriaType.cs \
	src/Searching/ItemSearchCriteria/EUSLSearchCriteria.cs \
	src/Searching/VolumeSearchCriteria/QuantitySearchCriteria.cs \
	src/Searching/VolumeSearchCriteria/QuantityField.cs \
	src/Searching/SearchUtils.cs \
	src/Searching/FreeTextSearchCriteria.cs \
	src/Searching/IFreeTextSearchField.cs \
	src/Searching/VolumeSearchCriteria/FreeTextSearchField.cs \
	src/Import/GnomeCatalogImport.cs \
	src/Import/BasenjiImport.cs \
	src/Import/AbstractImport.cs \
	src/Import/Events.cs \
	src/Import/CdCollectImport.cs \
	src/Import/IImport.cs \
	src/DecoderUtility.cs \
	src/DbData.cs \
	src/VolumeScanner/ScannerOptions.cs \
	src/VolumeScanner/FilesystemScannerOptions.cs \
	src/VolumeScanner/VolumeProber.cs \
	src/VolumeScanner/AudioCdVolumeScanner.cs \
	src/VolumeScanner/AudioCdVolumeInfo.cs \
	src/VolumeScanner/AudioCdScannerOptions.cs \
	src/AudioTrackVolumeItem.cs \
	src/AudioCdVolume.cs \
	src/AudioCdRootVolumeItem.cs \
	src/Searching/VolumeSearchCriteria/EUSLSearchCriteria.cs \
	src/Searching/AbstractEUSLSearchCriteria.cs \
	src/Global.cs \
	src/Metadata/MetadataProvider.cs \
	src/Metadata/MetadataType.cs \
	src/Metadata/MetadataItem.cs \
	src/Metadata/MetadataStore.cs \
	src/Metadata/MetadataUtils.cs \
	src/Metadata/ArchiveMetadataProvider.cs \
	src/Metadata/TagLibMetadataProvider.cs \
	src/Import/CdCatImport.cs \
	src/Import/ImportException.cs \
	src/Import/ImportAttribute.cs 

DATA_FILES = 

RESOURCES = \
	po/de.resources,VolumeDB.po.de.resources \
	po/he.resources,VolumeDB.po.he.resources \
	po/da.resources,VolumeDB.po.da.resources \
	po/fr.resources,VolumeDB.po.fr.resources \
	po/es.resources,VolumeDB.po.es.resources \
	po/it.resources,VolumeDB.po.it.resources \
	po/bg.resources,VolumeDB.po.bg.resources \
	po/oc.resources,VolumeDB.po.oc.resources \
	po/pt_BR.resources,VolumeDB.po.pt_BR.resources \
	po/tr.resources,VolumeDB.po.tr.resources \
	po/pl.resources,VolumeDB.po.pl.resources \
	po/hu.resources,VolumeDB.po.hu.resources \
	po/et.resources,VolumeDB.po.et.resources \
	po/zh_CN.resources,VolumeDB.po.zh_CN.resources \
	po/sr.resources,VolumeDB.po.sr.resources \
	po/ru.resources,VolumeDB.po.ru.resources \
	po/hr.resources,VolumeDB.po.hr.resources \
	po/cs.resources,VolumeDB.po.cs.resources \
	po/nl.resources,VolumeDB.po.nl.resources 

EXTRAS = \
	volumedb.pc.in 

REFERENCES =  \
	System \
	System.Data \
	System.Core \
	-pkg:taglib-sharp \
	ICSharpCode.SharpZipLib \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL))
$(eval $(call emit-deploy-target,PLATFORM_COMMON_DB_DLL_MDB))
$(eval $(call emit-deploy-target,PLATFORM_DLL))
$(eval $(call emit-deploy-target,PLATFORM_DLL_CONFIG))
$(eval $(call emit-deploy-target,PLATFORM_DLL_MDB))
$(eval $(call emit-deploy-target,MUSICBRAINZ_DLL))
$(eval $(call emit-deploy-target,MUSICBRAINZ_DLL_MDB))
$(eval $(call emit-deploy-wrapper,VOLUMEDB_PC,volumedb.pc))


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
	$(call cp,$(VOLUMEDB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_COMMON_DB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_COMMON_DB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MUSICBRAINZ_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MUSICBRAINZ_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(VOLUMEDB_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(VOLUMEDB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_COMMON_DB_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_COMMON_DB_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_CONFIG),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PLATFORM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MUSICBRAINZ_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MUSICBRAINZ_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(VOLUMEDB_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
