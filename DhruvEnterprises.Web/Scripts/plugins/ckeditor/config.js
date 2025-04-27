/**
 * @license Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
    // config.uiColor = '#AADC6E';
    // The toolbar groups arrangement, optimized for two toolbar rows.
    config.allowedContent = true;
    config.toolbarGroups = [
		{ name: 'clipboard', groups: ['clipboard', 'undo'] },
		{ name: 'editing', groups: ['find', 'selection', 'spellchecker'] },
		{ name: 'links' },
		{ name: 'insert' },
		{ name: 'forms' },
		{ name: 'tools' },
		{ name: 'document', groups: ['mode', 'document', 'doctools'] },
		{ name: 'others' },
    //'/',
		{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
    //	{ name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'] },
        	{ name: 'paragraph', groups: ['list', 'align'] },
		{ name: 'styles' },
		{ name: 'colors' }//,
    //	{ name: 'about' }
    ];

    config.toolbar_Basic = [
	    ['Source', 'Bold', 'Italic', 'Underline', 'Cut', 'Copy', 'Paste', 'Undo', 'Redo', '-', 'NumberedList', 'BulletedList', '-', 'Link', 'Unlink', '-', 'Print', '-', 'About']
    ];

    //For font awsome
    config.protectedSource.push(/<i class[\s\S]*?\>/g);
    config.protectedSource.push(/<\/i>/g);

    //config.allowedContent = true;
    //config.protectedSource.push(/\{[\s\S]*?\}/g);
    //config.disallowedContent = true;
};
CKEDITOR.dtd.$removeEmpty['i'] = false;
