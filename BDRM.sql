CREATE DATABASE RoryMercury
USE RoryMercury

CREATE TABLE telegramAccount
(
	telegramId INT NOT NULL,
	userName VARCHAR(25) NOT NULL,
	userToken VARCHAR(45) NOT NULL,
	PRIMARY KEY(telegramId)
)

CREATE TABLE clientApps
(
	machineId INT IDENTITY(1,1) NOT NULL,
	telegramId INT NOT NULL,
	userLogin VARCHAR(50) NOT NULL,
	userPassword VARCHAR(50) NOT NULL,
	ipMachine VARCHAR(20) NOT NULL,
	macAddress VARCHAR(60) NOT NULL,
	PRIMARY KEY(machineId),
	FOREIGN KEY (telegramId) REFERENCES telegramAccount (telegramId)
)