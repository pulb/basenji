post-install-local-hook:
	cp -R $(BUILD_DIR)/data $(DESTDIR)$(libdir)/$(PACKAGE);

