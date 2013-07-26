/*
SQLyog Community v11.1 (64 bit)
MySQL - 5.6.10-log : Database - emotive
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`emotive` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `emotive`;

/*Table structure for table `roles` */

DROP TABLE IF EXISTS `roles`;

CREATE TABLE `roles` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `roles` */

/*Table structure for table `userhasroles` */

DROP TABLE IF EXISTS `userhasroles`;

CREATE TABLE `userhasroles` (
  `UserId` int(11) NOT NULL,
  `RoleId` int(11) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `userhasroles` */

/*Table structure for table `users` */

DROP TABLE IF EXISTS `users`;

CREATE TABLE `users` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(20) DEFAULT NULL,
  `Forename` varchar(30) DEFAULT NULL,
  `Surname` varchar(30) DEFAULT NULL,
  `Email` varchar(70) DEFAULT NULL,
  `Enabled` tinyint(1) DEFAULT '1',
  `Archived` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8;

/*Data for the table `users` */

insert  into `users`(`ID`,`Username`,`Forename`,`Surname`,`Email`,`Enabled`,`Archived`) values (1,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(2,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(3,'chris','chris','chris','chris.withers@gmail.com',1,0),(4,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(5,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(6,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(7,'Chris','chriis','chris','chris.withers@gmail.com',1,0),(8,'Chris','chriis','chris','chris.withers@gmail.com',1,0);

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
