Experiment surrounding CQRS(Command Query Responsibility Segregation) and Event Sourcing

##Steps

<ol>
	<li>Click View --> Server Explorer</li>
	<li>Highlight <b>Data Connections</b> and right click</li>
	<li>Select <b>Create new SQL Server Database</b></li>
	<li>Enter the following:
		<ul>
			<li>Server Name: (localdb)\MSSQLLocalDB</li>
			<li>New Database Name: AccountingReadModel</li>
		</ul>
	</li>
	<li>After the database is created, open the <b>App.config</b> within InvoiceCqrs.Migrations</li>
	<li>Change the connection string value <b>Server=(localdb)\MSSQLLocalDB</b>/li>
		<ul>
			<li>For more info on why this has to change: http://stackoverflow.com/questions/33270885/localdb-not-recognized-in-visual-studio-2015</li>
		</ul>
	<li>Also change the connection string info in the <b>App.config</b> within the InvoiceCqrs AND InvoiceCqrs.Web projects</li>
	<li>Rebuild the project</li>
	<li>Open powershell and path to the InvoiceCqrs project</li>
	<li>Run the migrate.ps1 file to setup the DB with the migration info</li>
	
</ol>