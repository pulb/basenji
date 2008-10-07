post-install-local-hook:
	cp -R $(BUILD_DIR)/data $(DESTDIR)$(libdir)/$(PACKAGE);

post-uninstall-local-hook:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE)/data;
