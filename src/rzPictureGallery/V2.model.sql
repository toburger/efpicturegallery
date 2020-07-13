create table pictures
	( filename string(256)
	, gallery string(256) null references galleries(name) on delete set null
	, constraint pk_pictures primary key (filename, gallery)
	);

create index ix_pictures_gallery on pictures
	( gallery
	);
