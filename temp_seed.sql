DO $$
DECLARE
    suffix text := substr(md5(random()::text), 1, 6);
    client_id int;
    cat_ids int[];
    p1 int; p2 int; p3 int; p4 int; p5 int;
    sr1 int; sr2 int; sr3 int; sr4 int; sr5 int;
BEGIN
    IF (SELECT COUNT(*) FROM "Categories") < 5 THEN
        INSERT INTO "Categories" ("Name","Description","Icon","IsActive","CreatedAt") VALUES
            ('Eletrica', 'Servicos eletricos residenciais', 'eletrica', true, now()),
            ('Pintura', 'Pintura e acabamento', 'pintura', true, now()),
            ('Hidraulica', 'Reparo e manutencao hidraulica', 'hidraulica', true, now()),
            ('Jardinagem', 'Jardinagem e paisagismo', 'jardinagem', true, now()),
            ('Limpeza', 'Limpeza residencial e pos-obra', 'limpeza', true, now())
        ON CONFLICT ("Name") DO NOTHING;
    END IF;

    SELECT array_agg("Id" ORDER BY "Id") FROM "Categories" LIMIT 5 INTO cat_ids;
    IF array_length(cat_ids, 1) < 5 THEN
        RAISE NOTICE 'Ainda nao ha 5 categorias cadastradas.';
        RETURN;
    END IF;

    INSERT INTO "Clients" ("Name","Email","Phone","Address","CreatedAt","IsActive")
    VALUES ('Cliente Demo ' || suffix, 'cliente.demo.' || suffix || '@example.com', '(11) 90000-0000', 'Rua Teste, 123', now(), true)
    RETURNING "Id" INTO client_id;

    INSERT INTO "Professionals" ("Name","Email","Phone","Address","Description","CategoryId","CreatedAt","IsActive")
    VALUES ('Ana Martins', 'ana.' || suffix || '@example.com', '(11) 95555-0001', 'Av. Central, 10 - SP', 'Eletricista residencial', cat_ids[1], now(), true)
    RETURNING "Id" INTO p1;

    INSERT INTO "Professionals" ("Name","Email","Phone","Address","Description","CategoryId","CreatedAt","IsActive")
    VALUES ('Bruno Costa', 'bruno.' || suffix || '@example.com', '(21) 96666-0002', 'Rua das Flores, 20 - RJ', 'Pintor e acabamento', cat_ids[2], now(), true)
    RETURNING "Id" INTO p2;

    INSERT INTO "Professionals" ("Name","Email","Phone","Address","Description","CategoryId","CreatedAt","IsActive")
    VALUES ('Carla Nunes', 'carla.' || suffix || '@example.com', '(31) 97777-0003', 'Av. Brasil, 30 - BH', 'Encanadora emergencial', cat_ids[3], now(), true)
    RETURNING "Id" INTO p3;

    INSERT INTO "Professionals" ("Name","Email","Phone","Address","Description","CategoryId","CreatedAt","IsActive")
    VALUES ('Diego Lima', 'diego.' || suffix || '@example.com', '(41) 98888-0004', 'Rua Verde, 40 - CTBA', 'Jardinagem e paisagismo', cat_ids[4], now(), true)
    RETURNING "Id" INTO p4;

    INSERT INTO "Professionals" ("Name","Email","Phone","Address","Description","CategoryId","CreatedAt","IsActive")
    VALUES ('Eduarda Prado', 'eduarda.' || suffix || '@example.com', '(51) 99999-0005', 'Av. Sul, 50 - POA', 'Limpeza pos-obra', cat_ids[5], now(), true)
    RETURNING "Id" INTO p5;

    INSERT INTO "ServiceRequests" ("ClientId","ProfessionalId","Title","Description","Status","CreatedAt","IsActive")
    VALUES (client_id, p1, 'Troca de disjuntor',   'Ajuste no quadro', 4, now(), true) RETURNING "Id" INTO sr1;
    INSERT INTO "ServiceRequests" ("ClientId","ProfessionalId","Title","Description","Status","CreatedAt","IsActive")
    VALUES (client_id, p2, 'Pintura de sala',      'Parede e teto',    4, now(), true) RETURNING "Id" INTO sr2;
    INSERT INTO "ServiceRequests" ("ClientId","ProfessionalId","Title","Description","Status","CreatedAt","IsActive")
    VALUES (client_id, p3, 'Desentupimento',       'Banheiro social',  4, now(), true) RETURNING "Id" INTO sr3;
    INSERT INTO "ServiceRequests" ("ClientId","ProfessionalId","Title","Description","Status","CreatedAt","IsActive")
    VALUES (client_id, p4, 'Poda de jardim',       'Area externa',     4, now(), true) RETURNING "Id" INTO sr4;
    INSERT INTO "ServiceRequests" ("ClientId","ProfessionalId","Title","Description","Status","CreatedAt","IsActive")
    VALUES (client_id, p5, 'Limpeza pos-obra',     'Apartamento 80m2', 4, now(), true) RETURNING "Id" INTO sr5;

    INSERT INTO "Reviews" ("ClientId","ProfessionalId","ServiceRequestId",
                           "PriceRating","QualityRating","SpeedRating","CommunicationRating","ProfessionalismRating",
                           "Comment","PositivePoints","NegativePoints","CreatedAt","IsActive")
    VALUES
        (client_id, p1, sr1, 4, 5, 4, 5, 4, 'Bom servico eletrico', 'Rapido e cuidadoso', 'Preco acima do esperado', now(), true),
        (client_id, p2, sr2, 5, 5, 5, 5, 5, 'Pintura impecavel',    'Acabamento perfeito', 'Nenhuma',               now(), true),
        (client_id, p3, sr3, 3, 3, 3, 3, 3, 'Resolveu mas demorou','Atendeu de madrugada','Atraso na chegada',     now(), true),
        (client_id, p4, sr4, 4, 4, 5, 4, 4, 'Jardim ficou otimo',   'Capricho na limpeza', 'Preco um pouco alto',   now(), true),
        (client_id, p5, sr5, 2, 2, 2, 3, 2, 'Deixou a desejar',     'Educada',             'Faltou detalhe',        now(), true);

    RAISE NOTICE 'Profissionais, solicitacoes e avaliacoes inseridos (sufixo %).', suffix;
END $$;
