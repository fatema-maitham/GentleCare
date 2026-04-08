using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Specializations_SpecializationId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SpecializationId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_VisitRecords_AppointmentId",
                table: "VisitRecords",
                newName: "UQ_VisitRecord_Appointment");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Specializations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CPRNumber",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Doctors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Doctors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Specialization_Name",
                table: "Specializations",
                column: "Name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Prescription_Duration",
                table: "Prescriptions",
                sql: "DurationDays > 0");

            migrationBuilder.CreateIndex(
                name: "UQ_Patient_CPRNumber",
                table: "Patients",
                column: "CPRNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Patient_ReferenceNumber",
                table: "Patients",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_DoctorSchedule_SlotDuration",
                table: "DoctorSchedules",
                sql: "SlotDurationMinutes > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DoctorSchedule_Times",
                table: "DoctorSchedules",
                sql: "EndTime > StartTime");

            migrationBuilder.CreateIndex(
                name: "UQ_Doctor_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Appointment_Doctor_DateTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Specialization_Name",
                table: "Specializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Prescription_Duration",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "UQ_Patient_CPRNumber",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "UQ_Patient_ReferenceNumber",
                table: "Patients");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DoctorSchedule_SlotDuration",
                table: "DoctorSchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DoctorSchedule_Times",
                table: "DoctorSchedules");

            migrationBuilder.DropIndex(
                name: "UQ_Doctor_LicenseNumber",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "UQ_Appointment_Doctor_DateTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Doctors");

            migrationBuilder.RenameIndex(
                name: "UQ_VisitRecord_Appointment",
                table: "VisitRecords",
                newName: "IX_VisitRecords_AppointmentId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Specializations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CPRNumber",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "SpecializationId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SpecializationId",
                table: "Appointments",
                column: "SpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Specializations_SpecializationId",
                table: "Appointments",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
