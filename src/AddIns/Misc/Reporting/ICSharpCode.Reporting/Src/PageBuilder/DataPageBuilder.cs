﻿/*
 * Created by SharpDevelop.
 * User: Peter Forstmeier
 * Date: 06.06.2013
 * Time: 20:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ICSharpCode.Reporting.DataManager.Listhandling;
using ICSharpCode.Reporting.Interfaces;
using ICSharpCode.Reporting.Interfaces.Export;
using ICSharpCode.Reporting.PageBuilder.Converter;
using ICSharpCode.Reporting.PageBuilder.ExportColumns;

namespace ICSharpCode.Reporting.PageBuilder
{
	/// <summary>
	/// Description of DataPageBuilder.
	/// </summary>
	public class DataPageBuilder:BasePageBuilder
	{
		public DataPageBuilder(IReportModel reportModel, Type elementType,IEnumerable list):base(reportModel)
		{
			List = list;
			ElementType = elementType;
		}
		
		
		public override void BuildExportList()
		{
			base.BuildExportList();
			CurrentPage = CreateNewPage ();
			WriteStandardSections();
			CurrentLocation = DetailStart;
			BuildDetail();
			base.AddPage(CurrentPage);
		}
		
		
		void aaBuildDetail()
		{
			
			Container = ReportModel.DetailSection;
			var collectionSource = new CollectionSource(List,ElementType,ReportModel.ReportSettings);
			IExportContainer detail = null;
			if(collectionSource.Count > 0) {
				collectionSource.Bind();
				CurrentLocation = DetailStart;
				
				var position = ResetPosition();
				var converter = new ContainerConverter(base.Graphics, CurrentLocation);
//				var converter = new ContainerConverter(base.Graphics, position);
				detail = CreateContainerForSection(DetailStart);

				
				do {
					collectionSource.Fill(Container.Items);
					var convertedItems = converter.CreateConvertedList(ReportModel.DetailSection,detail,position);
					if (PageFull(convertedItems)) {
						detail.ExportedItems.AddRange(convertedItems);
						CurrentPage.ExportedItems.Insert(2,detail);
						Pages.Add(CurrentPage);
						MeasureAndArrangeContainer(converter,detail);

						position = ResetPosition();
						CurrentPage = CreateNewPage();
						WriteStandardSections();
						CurrentLocation = DetailStart;
						detail = CreateContainerForSection(DetailStart);
						
					} else {
						detail.ExportedItems.AddRange(convertedItems);
						MeasureAndArrangeContainer(converter,detail);
						position = new Point(Container.Location.Y,position.Y + Container.Size.Height);
					}
				}
				
				while (collectionSource.MoveNext());
				InsertDetailAtPosition(detail);
				base.BuildReportFooter();
				
			} else {
				detail = CreateContainerForSection(DetailStart);
				InsertDetailAtPosition(detail);
				base.BuildReportFooter();
			}
		}
		
		
		void BuildDetail()
		{
			
			Container = ReportModel.DetailSection;
			var collectionSource = new CollectionSource(List,ElementType,ReportModel.ReportSettings);
			IExportContainer detail = null;
			if(collectionSource.Count > 0) {
				collectionSource.Bind();

				var position = DetailStart;
				var converter = new ContainerConverter(base.Graphics, CurrentLocation);
				detail = CreateDetail(DetailStart);

				do {
					
					var row = CreateContainerIfNotExist(Container,detail, position);
					collectionSource.Fill(Container.Items);

					var convertedItems	 =  converter.CreateConvertedList(ReportModel.DetailSection,row,position);
					MeasureAndArrangeContainer(converter,row);
					row.ExportedItems.AddRange(convertedItems);
					if (PageFull(convertedItems)) {
						InsertDetailAtPosition(detail);
						Pages.Add(CurrentPage);
						CurrentPage = CreateNewPage();
						WriteStandardSections();
						position = ResetPosition();
						detail = CreateDetail(DetailStart);
						CurrentLocation = DetailStart;
						
						row = CreateContainerIfNotExist(Container,detail,position);
						var recreate =  converter.CreateConvertedList(ReportModel.DetailSection,row,position);
						MeasureAndArrangeContainer(converter,row);
						row.ExportedItems.AddRange(recreate);
					}
					detail.ExportedItems.Add(row);
					position = new Point(Container.Location.Y,position.Y + Container.Size.Height);
				}
				
				while (collectionSource.MoveNext());
				InsertDetailAtPosition(detail);
//				base.BuildReportFooter();
				
			} else {
				detail = CreateContainerForSection(DetailStart);
				InsertDetailAtPosition(detail);
				base.BuildReportFooter();
			}
		}

	
		
		IExportContainer CreateContainerIfNotExist(IReportContainer container, IExportContainer parent, Point position)
		{
			var row = CreateContainerForSection(position);
				row.Name = "Row";
				row.Parent = parent;
				row.Location = new Point(50, position.Y);
				row.Size = new Size(400, 40);
				row.BackColor = Color.Green;
				return row;
		}

		
		IExportContainer CreateDetail(Point startLocation)
		{
			var detail = CreateContainerForSection(startLocation);
			detail.Parent = CurrentPage;
			return detail;
		}

		
		
		Point ResetPosition () {
			return DetailStart;
		}
		
		
		void MeasureAndArrangeContainer(IContainerConverter converter,IExportContainer container)
		{
			converter.Measure(container);
			converter.ArrangeContainer(container);
		}

		
		IExportContainer CreateContainerForSection(Point location )
		{
			var detail = (ExportContainer)Container.CreateExportColumn();
			detail.Location = location;
			return detail;
		}
		
		
		void InsertDetailAtPosition(IExportContainer container)
		{
			if (Pages.Count == 0) {
				CurrentPage.ExportedItems.Insert(2, container);
			} else {
				CurrentPage.ExportedItems.Insert(1, container);
			}
		}
		
		
		internal IReportContainer Container { get; private set; }
		
		public IEnumerable List {get; private set;}
		
		public Type ElementType {get;private set;}
	}
}