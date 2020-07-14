create table gallery_tags
    ( tag string(256)
    , gallery string(256) references galleries(name) on delete cascade
    , constraint pk_gallery_tags primary key (tag, gallery)
    );
