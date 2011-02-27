# patch src/App.cs to load themes and icons from prefix/share/basenji
APP_CS=$(srcdir)/src/App.cs
$(eval $(call emit-deploy-wrapper, APP_CS, $(srcdir)/src/App.cs))

post-install-local-hook:
	# copy themes and icons into prefix/share/basenji
	if [ ! -d $(DESTDIR)$(datadir)/$(PACKAGE) ]; then \
		mkdir -p $(DESTDIR)$(datadir)/$(PACKAGE); \
	fi; \
	cp -R $(BUILD_DIR)/data/* $(DESTDIR)$(datadir)/$(PACKAGE); \
	\
	gio_assembly_path="`pkg-config --variable=Libraries gio-sharp-2.0`"; \
	gio_assembly_name="`basename $$gio_assembly_path`"; \
	ln -s "$$gio_assembly_path" -n $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name"; \
	ln -s "$$gio_assembly_path.config" -n $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name.config";

post-uninstall-local-hook:
	# remove prefix/share/basenji
	if [ -d $(DESTDIR)$(datadir)/$(PACKAGE) ]; then	\
		rm -rf $(DESTDIR)$(datadir)/$(PACKAGE); \
	fi; \
	\
	gio_assembly_path="`pkg-config --variable=Libraries gio-sharp-2.0`"; \
	gio_assembly_name="`basename $$gio_assembly_path`"; \
	if [ -f $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name" ]; then \
		rm $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name"; \
	fi; \
	if [ -f $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name.config" ]; then \
		rm $(DESTDIR)$(libdir)/$(PACKAGE)/"$$gio_assembly_name.config"; \
	fi;
