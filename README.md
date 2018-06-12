# Sheng.SQLite.Plus

Sheng.SQLite.Plus 是一个对直接使用 ADO.NET 方式操作 SQLite 数据库的一个增强组件，它的操作方式介于 Entity Framework 和 ADO.NET 之间，是用于 SQLite 的高度自由和高开发效率的数据库访问层组件。

+ 支持所有 ADO.NET 原生操作
+ 由开发人员定义模型并解除与数据库表一一对应的关系，可由开发人员灵活指定映射关系。同一张表可以对应到多个不同的模型。
+ 支持直接使用 SQL 语句并根据查询结果在内存中动态映射数据到模型。
+ 在批量操作数据时，支持自动化的事务处理，可自动回滚。
+ 支持一对多的映射关系，即一个实体类可以映射到多张表，反之亦可。
+ 支持自动填充/补全数据实体类中的数据，声明模型并给定主键值或其它条件后，可自动填充模型。
+ 支持 DataSet、DataTable、DataRow 多种粒种的内存动态映射，直接从这些数据集合中生成强类型的对象集合。
+ 支持简单 SQL 构造器，支持自动生成简单的无模型映射的 SQL 语句。
+ 支持对实体字段的精细化处理，如将实体对象的任意 Property 标记 JsonAttribute 后，将自动以 Json 格式写入或读取字段。
+ 高性能，高灵活性，高可维护性。

更好的排版和详细的使用说明见这里：
http://blog.shengxunwei.com/Home/Post/5364bf7e-07a8-4daf-b5bd-9bb4645bb739


曹旭升（sheng.c）  
QQ:279060597  
Email：cao.silhouette@msn.com  
@南京 

