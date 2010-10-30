# patch src/App.cs to load themes and icons from prefix/share/basenji
APP_CS=$(srcdir)/src/App.cs
$(eval $(call emit-deploy-wrapper, APP_CS, $(srcdir)/src/App.cs))

post-install-local-hook:
	# copy themes and icons into prefix/share/basenji
	if [ ! -d $(DESTDIR)$(datadir)/$(PACKAGE) ]; then \
		mkdir -p $(DESTDIR)$(datadir)/$(PACKAGE); \
	fi; \
	cp -R $(BUILD_DIR)/data/* $(DESTDIR)$(datadir)/$(PACKAGE);

post-uninstall-local-hook:
	# remove prefix/share/basenji
	if [ -d $(DESTDIR)$(datadir)/$(PACKAGE) ]; then	\
		rm -rf $(DESTDIR)$(datadir)/$(PACKAGE); \
	fi;
