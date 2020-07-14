drop view galleries_with_tags;
create view galleries_with_tags as
    select
        coalesce(g.name, '') id             -- for de-duplication
        , coalesce(g.name, null) name         -- coalesce is required
        , g.url
        , count(*) count
        , max(p.created) last_update
        , many tags (gt.tag)                -- navigation property
    from pictures p
    left join galleries g on p.gallery = g.name
    left join gallery_tags gt on g.name = gt.gallery
    group by g.name, g.url, gt.tag;
