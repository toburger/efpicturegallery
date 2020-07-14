create view galleries_with_tags as
    select g.name, g.url, count(*) count, max(p.created) last_update, gt.tag from pictures p
    left join galleries g on p.gallery = g.name
    left join gallery_tags gt on g.name = gt.gallery
    group by g.name, g.url, gt.tag